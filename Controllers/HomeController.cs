using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using YemekTarifleri.Db;
using YemekTarifleri.Models;
using YemekTarifleri.Services;


namespace YemekTarifleri.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserManager _userManager;
        private readonly Services.IRecipeManager _recipeManager;
        private readonly LocationService _locationService;


        public HomeController(ILogger<HomeController> logger, Services.IUserManager userManager, IRecipeManager recipeManager, LocationService locationService)
        {
            _logger = logger;
            _userManager = userManager;
            _recipeManager = recipeManager;
            _locationService = locationService;
        }
        [HttpPost]
        public async Task<IActionResult> SetLocation([FromBody] CountryModel location)
        {
            if (location == null)
            {
                return BadRequest("Invalid location data.");
            }

            var userName = User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("User not logged in.");
            }

            var user = await Util.user.GetUserByUserName(userName);

            if (user == null)
            {
                return BadRequest("User not found.");
            }

            var userId = user.UserId;

            // IP adresini al
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Kullanýcý konumunu ayarla
            await SetUserLocation(ipAddress, userId);

            // Konumu oturuma kaydet
            HttpContext.Session.SetString("UserLocation", JsonConvert.SerializeObject(location));

            bool result = await Util.Country.SaveCountryAsync(location, userId);

            if (result)
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "Failed to save country." });
            }
        }

        private async Task SetUserLocation(string ipAddress, int userId)
        {
            var (city, countryName) = await _locationService.GetLocationAsync(ipAddress);

            var countryModel = new CountryModel
            {
                CountryName = countryName,
                City = city,
            };

            bool result = await Util.Country.SaveCountryAsync(countryModel, userId);

            if (result)
            {
                Console.WriteLine("User location saved successfully.");
            }
            else
            {
                Console.WriteLine("Failed to save user location.");
            }
        }




        public class LocationService
        {
            private static readonly HttpClient httpClient = new HttpClient();
            private readonly string apiToken;

            // Constructor to accept IConfiguration
            public LocationService(IConfiguration configuration)
            {
                apiToken = configuration["IpInfoApiToken"];
            }

            public async Task<(string City, string CountryName)> GetLocationAsync(string ipAddress)
            {
                try
                {
                    string apiUrl = $"https://ipinfo.io/{ipAddress}/json?token={apiToken}";

                    HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(responseBody);

                    string city = json["city"]?.ToString();
                    string countryName = json["country"]?.ToString();

                    return (city, countryName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    return (null, null); // Default values on failure
                }
            }
        }






        [HttpPost]
        public IActionResult SkipLocation()
        {
            HttpContext.Session.SetString("UserLocationSkipped", "true");
            return Json(new { success = true });
        }



        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userName = User.Identity.Name;

                // Check if the user needs to be prompted for location
                var promptLocation = HttpContext.Session.GetString("PromptLocation");

                if (promptLocation == "true")
                {
                    // Remove the flag after processing
                    HttpContext.Session.Remove("PromptLocation");
                    return View("LocationPrompt"); // Redirect to location prompt view
                }
            }

            // Fetch recipes for the index view
            var recipes = Util.Recipe.GetRecipesByModel();
            ViewBag.Location = HttpContext.Session.GetString("UserLocation");

            // Return the view with recipes
            return View(recipes);
        }




        public IActionResult LocationPrompt()
        {
            return View();
        }




        [HttpGet]
        public IActionResult GetImage(int imageID)
        {
            using (YemekTarifleriContext _context = new YemekTarifleriContext())
            {
                var image = _context.Images.FirstOrDefault(f => f.ImageId == imageID);
                if (image == null)
                {
                    return NotFound();
                }

                // Assuming the image data is stored in image.Data and the content type in image.ContentType
                return File(image.ImageContent, "image/*");
            }
        }
       

        public IActionResult AccessDenied()
        {
            return View();
        }
      
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }
        public IActionResult Dishes()
        {
            return View();
        }

        public IActionResult GetRecipeImage(int imageID)
        {
            using (var context = new YemekTarifleriContext())
            {
                var image = context.Images
                    .Where(i => i.ImageId == imageID)
                    .Select(i => new
                    {
                        i.ImageContent,
                        i.FileName
                    })
                    .FirstOrDefault();

                if (image != null && image.ImageContent != null)
                {
                    var mimeType = "image/jpeg"; // veya dosya uzantýsýna göre dinamik olarak belirleyebilirsiniz
                    return File(image.ImageContent, mimeType);
                }

                return NotFound(); // Veya varsayýlan bir resim dönebilirsiniz
            }
        }




        [HttpGet]
        public async Task<IActionResult> OrderByMinutes()
        {
            using (var context = new YemekTarifleriContext())
            {
                // Tarifleri toplam süreye göre sýralama
                var sortedRecipes = await context.Recipes
                    .OrderBy(r => (r.PrepTime ?? 0) + (r.CookTime ?? 0)) // Süreleri toplama
                    .Select(r => new RecipeViewModel
                    {
                        RecipeId = r.RecipeId,
                        RecipeName = r.RecipeName ?? "Unknown",
                        TotalTime = (r.PrepTime ?? 0) + (r.CookTime ?? 0),
                        ImageId = r.ImageId 
                    })
                    .ToListAsync();

                if (sortedRecipes == null || !sortedRecipes.Any())
                {
                    return NotFound("No recipes found.");
                }

                return View(sortedRecipes); // "OrderByMinutes" adýnda bir view dosyanýz olmalý
            }
        }

        public IActionResult Search(string search)
        {
            var hasRecipes = _recipeManager.GetRecipes(search);

            if (hasRecipes == null)
            {
                // Handle the case where no recipes are found
                TempData["error"] = "No recipes found matching the search criteria.";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Handle the case where recipes are found
                // You might want to return a view or some other result here
                return RedirectToAction("Detail","Recipe", new { search }); // or return some other result depending on your requirement
            }
        }
        [Authorize(Roles = "Admin")]
        public IActionResult AdminOnlyAction()
        {
            // This action is only accessible to users with the Admin role
            return View();
        }





        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
