using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;
using YemekTarifleri.Db;
using YemekTarifleri.Models;
//using System.Web.Security;

namespace YemekTarifleri.Controllers
{
    public class AccountController : Controller
    {
        private readonly Services.IUserManager _userManager;
        //private readonly HttpContext _httpContext;
 

        public AccountController(Services.IUserManager userManager)
        {
            _userManager = userManager;
         
        }
        //public async Task<IActionResult> UserProfile()
        //{
        //    var model = new UserLoginModel
        //    {
        //        UserName = User.Identity.Name,
        //        UserRole = User.IsInRole("Admin") ? "Admin" : (User.IsInRole("Moderator") ? "Moderator" : "User")
        //    };

        //    return View(model);
        //}

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ViewAllUsers()
        {
            var users=Util.user.GetAllUsers();
            var userList = users.Select(u => new { userName = u.UserName, UserId = u.UserId , Email=u.Email }).ToList();
            return Json(userList);
        }

        [Authorize(Roles = "Moderator")]
        public async Task<IActionResult> ViewUsers()
        {
            var users = await Util.RoleManager.GetUsersInRoleAsync("User");
            var userList = users.Select(u => new { userName = u.UserName, UserId = u.UserId, Email = u.Email }).ToList();
            return Json(userList);
        }

        public IActionResult Profile()
        {
            var user = HttpContext.User;

            // Kullanıcının kimlik doğrulaması yapıldığından emin olun
            if (user == null || !user.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account"); // Kullanıcı giriş yapmamışsa, giriş sayfasına yönlendir
            }

            // Kullanıcı adı ve rolü al
            var userName = user.Identity.Name;
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

            // Kullanıcı ID'sini almak ve int türüne dönüştürmek
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId;
            if (!int.TryParse(userIdClaim, out userId))
            {
                // Debugging: Kullanıcı kimliği değeri nedir?
                // Bu noktada userIdClaim değişkeninin içeriğini kontrol edin
                return BadRequest($"User ID is not valid. ID claim value: {userIdClaim}");
            }

            // User bilgilerini ve rolünü view'e aktarıyoruz
            var userModel = new UserProfileViewModel
            {
                UserName = userName,
                UserId = userId,
                UserRole = userRole
            };

            return View(userModel);
        }


        [HttpGet]
        public IActionResult Login()
        {
            var response = new Models.UserLoginModel();
            return View(response);
        }



        [HttpGet]
        public IActionResult CreateUser()
        {
            var response = new Models.UserLoginModel();
            return View(response);
        }


        [HttpPost]
        public async Task<IActionResult> Login(UserLoginModel user)
        {
             if (!ModelState.IsValid)
            {
                return View(user);
            }
            int userId;
            var loginUser = _userManager.Login(user,out userId);

            if (loginUser != false)
            {

                var roles = await Util.RoleManager.GetRolesByUserIdAsync(userId);
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.NameIdentifier,user.Password),
       
            };
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.RoleName));
                }
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
                //_httpContext.Session.SetString("UserName", user.UserName);
      
                return RedirectToAction("Index", "Home");
            }

            TempData["error"] = "Wrong credentials. Please try again";
            return View(user);
        }



        [HttpPost]
        public async Task<IActionResult> CreateUser(UserLoginModel user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model state.");
            }

            // Attempt to create the user and retrieve the new user's ID
            int userId;
            var userCreationResult = _userManager.CreateUser(user, out userId);

            // Check if user creation was unsuccessful
            if (!userCreationResult)
            {
                return BadRequest("Username already exists or passwords do not match.");
            }

            // Set the flag to prompt for location
            HttpContext.Session.SetString("PromptLocation", "true");

            // Get the user's roles
            var roles = await Util.RoleManager.GetRolesByUserIdAsync(userId);
            // Prepare claims for the user
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()) // Use userId for the NameIdentifier claim
    };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.RoleName));
            }
            // Create claims identity and sign in the user
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            // Redirect to the index page, which will trigger the location prompt
            return RedirectToAction("Index", "Home");
        }




        [HttpGet]
        public IActionResult Logout()
        {
            return View(); // Render a view with a confirmation message
        }

        [HttpPost]
        public async Task<IActionResult> LogoutConfirmed()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [HttpDelete]
        public IActionResult DeleteUser(UserLoginModel user)
        {
            if (user == null)
            {
                return BadRequest("Invalid model state.");
            }

            bool result = _userManager.DeleteUser(user.UserId); // Adjust method accordingly
            if (!result)
            {
                TempData["ErrorMessage"] = "User not found or deletion failed.";
                return RedirectToAction("Profile", "Home");
            }

            HttpContext.SignOutAsync(); // This removes the authentication cookie

            TempData["SuccessMessage"] = "User successfully deleted.";
            return RedirectToAction("Login");
        }






    }
}
