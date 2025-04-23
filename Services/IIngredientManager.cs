using YemekTarifleri.Models;

namespace YemekTarifleri.Services
{
    public interface IIngredientManager
    {
        public bool add(IngredientModel ingredient);

        public bool remove(IngredientModel ingredient);
        

    }
}
