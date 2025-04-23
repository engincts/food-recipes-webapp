using Microsoft.AspNetCore.Mvc;

namespace YemekTarifleri.Controllers
{
    public class IngredientsController : Controller
    {
        private readonly Services.IIngredientManager _ingredientManager;

        public IngredientsController(Services.IIngredientManager ingredientManager)
        {
            _ingredientManager = ingredientManager;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Ingredient()
        {
            return View();
        }
    }
}
