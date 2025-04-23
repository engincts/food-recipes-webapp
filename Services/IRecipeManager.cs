

using Microsoft.AspNetCore.Mvc.RazorPages;
using YemekTarifleri.Db;
using YemekTarifleri.Models;

namespace YemekTarifleri.Services
{
    public interface IRecipeManager
    {
        public bool Create(RecipeViewModel model);
        //public bool Update(Models.RecipeViewModel model);
        //public bool Delete(Models.RecipeViewModel model);
        public List<Recipe> MenuList();
        public List<Db.Image> GetImage(int imageId);
        public bool Delete(int recipeId);
        public List<IngredientViewModel> GetIngredients(int recipeId);
        public RecipeIngredient AddIngredient(IngredientViewModel model);
        public bool DeleteIngredient(int recipeIngredientId);
        public RecipeIngredient UpdateIngredient(IngredientViewModel model);
        public List<Recipe> GetRecipes(string seacrhName);
    }
}
