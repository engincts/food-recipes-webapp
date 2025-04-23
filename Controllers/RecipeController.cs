using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using YemekTarifleri.Db;
using YemekTarifleri.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace YemekTarifleri.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RecipeController : Controller
    {

        private readonly Services.IRecipeManager _recipeManager;
        private readonly Services.ImageManager _imageManager;

        public RecipeController(Services.IRecipeManager recipeManager, Services.ImageManager imageManager)
        {
            _recipeManager = recipeManager;
            _imageManager = imageManager;
        }

        public IActionResult Menu()
        {
            return RedirectToAction("Create", "Recipe");
        }

        //[Authorize(Roles = "Admin,Moderator")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(RecipeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                return BadRequest("Invalid model state.");
            }

            _recipeManager.Create(model);

            return RedirectToAction("ShowMenu", "Recipe");
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



        [HttpGet]
        public IActionResult ShowMenu()
        {
            var list = _recipeManager.MenuList();
            return View(list);
        }

        [HttpPost]
        public IActionResult Delete(int recipeId)
        {
            var result = _recipeManager.Delete(recipeId);
            if (result)
            {
                return Ok();
            }

            return BadRequest("Failed to delete ingredients and recipe");
        }
        [HttpPost]
        public IActionResult UpdateIngredient(IngredientViewModel model)
        {
            if (model.RecipeIngredientId == 0)
            {
                return BadRequest("Ingredient ID is required");
            }

            try
            {
                var updatedIngredient = _recipeManager.UpdateIngredient(model);

                var response = new
                {
                    success = true,
                    updatedName = updatedIngredient.Ingredient.Name,
                    updatedAmount = updatedIngredient.Amount,
                    updatedUnit = updatedIngredient.Unit.Name
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult AddIngredient(IngredientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model state.");
            }

            var newRecipeIngredient = _recipeManager.AddIngredient(model);
            if (newRecipeIngredient == null)
            {
                throw new Exception("Ingredient not added");
            }

            // Create a response with necessary details
            var response = new
            {
                success = true,
                ingredientId = newRecipeIngredient.IngredientId,
                ingredientName = newRecipeIngredient.Ingredient.Name, // Assuming IngredientViewModel contains IngredientName
                amount = newRecipeIngredient.Amount,
                unitName = newRecipeIngredient.Unit.Name, // Assuming IngredientViewModel contains UnitName
            };

            return Json(response); // Return JSON response
        }




        [HttpPost]
        public List<IngredientViewModel> GetIngredients(int recipeId)
        {
            var ingredients = _recipeManager.GetIngredients(recipeId);

            return ingredients;
        }

        //public ActionResult Filter()
        //{

        //}
        public ActionResult IngredientTable()
        {
            // Assuming _recipeManager.MenuList() returns an IQueryable or List of Recipe entities
            var recipes = _recipeManager.MenuList();

            var model = recipes.Select(recipe => new RecipeViewModel
            {
                RecipeId = recipe.RecipeId,
                ImageId = recipe.ImageId,
                RecipeName = recipe.RecipeName,
                Description = recipe.Description,
                PrepTime = recipe.PrepTime,
                CookTime = recipe.CookTime,
                Servings = recipe.Servings,
                Rating = recipe.Rating,
                Steps = recipe.Steps,
                Ingredients = recipe.RecipeIngredients.Select(i => new IngredientViewModel
                {
                    RecipeIngredientId = i.RecipeIngredientId,
                    Name = i.Ingredient.Name,
                    Amount = i.Amount,
                    Unit = i.Unit?.Name,
                }).ToList()
            }).ToList();

            return View(model);
        }


        [HttpPost]
        public IActionResult DeleteIngredient(int recipeIngredientId)
        {
            var result = _recipeManager.DeleteIngredient(recipeIngredientId);
            if (result)
            {
                return Json(new { success = true, message = "Ingredient deleted" });
            }
            return Json(new { success = false, message = "Failed to delete ingredient" });
        }
        public IActionResult showImage(int imageId)
        {
            //var imagedata=_recipeManager.GetRecipes(imageId);
            //return View(imagedata,"image/jpg");
            return View();
        }

        public IActionResult Detail(string search)
        {
            using (var context = new YemekTarifleriContext())
            {
                // Ensure model is properly initialized
                var model = new RecipeViewModel();

                // Convert search string to lower case
                string lowerSearch = search?.ToLower();

                // Find the specific recipe that matches the search string
                var recipe = context.Recipes
                    .Include(r => r.Image) // Include the image associated with the recipe
                    .FirstOrDefault(r => string.IsNullOrEmpty(lowerSearch) || r.RecipeName.ToLower() == lowerSearch);

                if (recipe != null)
                {
                    // Populate the view model with the details of the selected recipe
                    model = new RecipeViewModel
                    {
                        RecipeName = recipe.RecipeName,
                        RecipeId = recipe.RecipeId,
                        PrepTime = recipe.PrepTime,
                        Rating = recipe.Rating,
                        Servings = recipe.Servings,
                        CookTime = recipe.CookTime,
                        Description = recipe.Description,
                        //ImageId = recipe.Image.ImageId
                        // Uncomment the following line if you need to include the image file content as Base64 string
                        // ImageFile = recipe.Image?.ImageContent != null ? Convert.ToBase64String(recipe.Image.ImageContent) : null
                    };
                }
                else
                {
                    // Handle the case where the recipe was not found
                    return NotFound("Recipe not found.");
                }

                return View(model);
            }
        }





    }
}

