using YemekTarifleri.Db;

namespace YemekTarifleri.Models
{
    public class RecipeViewModel
    {

        public int RecipeId { get; set; }
        public string RecipeName { get; set; }
        public string? Description { get; set; }
      
        public int? PrepTime { get; set; }

        public int? CookTime { get; set; }
        public int ImageId { get; set; }
        //public string Country {  get; set; }

        public IFormFile ImageFile { get; set; }

        public int TotalTime {  get; set; } // Total preparation and cooking time
        public int? Servings { get; set; }

        public int? Rating { get; set; }

        public string? Steps { get; set; }
        public List<IngredientViewModel> Ingredients { get; set; } = new List<IngredientViewModel>();
        public List<CountryModel> countryModels { get; set; }= new List<CountryModel>();
        //public List<ImageViewModel> Images { get; set; } = new List<ImageViewModel>();

       
    }

}
