using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YemekTarifleri.Db;
using YemekTarifleri.Models;

namespace YemekTarifleri.Controllers
{
    [Authorize(Roles ="Admin",Policy = "AdminPolicy")]
    public class Destination : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Destinations() 
        {
            var recipes=Util.Recipe.MenuList();
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
    }
}
