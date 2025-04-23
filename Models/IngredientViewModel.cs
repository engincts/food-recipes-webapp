namespace YemekTarifleri.Models
{
    public class IngredientViewModel
    {
        public int IngredientId { get; set; }
        public int RecipeIngredientId { get; set; }
        public int RecipeId { get; set; }
        public string? Name {  get; set; }
        public decimal? Amount {  get; set; }
        public string? Unit { get; set; }
        public int UnitId { get; set; }


    }
}
