namespace YemekTarifleri.Models
{
    public class ImageViewModel
    {
        public int ImageId { get; set; }
        public string ImagePath { get; set; } 
        public string? FileName { get; set; }
        public List<RecipeViewModel> recipes { get; set; }=new List<RecipeViewModel>();
    }
}
