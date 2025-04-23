namespace YemekTarifleri.Models
{
    public class Review
    {
        public string UserId { get; set; }
        public string RecipeId { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }//(1-5 arası)

    }
}
