using YemekTarifleri.Db;

namespace YemekTarifleri.Models
{
    public class ShoppingList
    {
        public string UserId { get; set; }
        public List<Ingredient> Items { get; set; }
    }
}
