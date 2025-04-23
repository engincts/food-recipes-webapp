using YemekTarifleri.Repository;

namespace YemekTarifleri
{
    public class Util
    {
        static Repository.RecipeRepository? _recipe;

        public static Repository.RecipeRepository Recipe
        {
            get
            {
                if (_recipe == null)
                {
                    _recipe = new Repository.RecipeRepository();
                }

                return _recipe;
            }
        }
        static Repository.UserRepository? _user;
        public static Repository.UserRepository user
        {
            get
            {
                if (_user == null)
                {
                    _user = new Repository.UserRepository();
                }
                return _user;
            }

        }

        static Repository.ImageRepository? _image;
        public static Repository.ImageRepository image
        {
            get
            {
                if( _image == null)
                {
                    _image = new Repository.ImageRepository();
                }
                return _image;
                    
            }
            
        }

        static CountryRepository? _country;

        public static CountryRepository Country
        {
            get
            {
                if (_country == null)
                    _country = new CountryRepository();
                return _country;
            }
        }
        static RoleManager? roleManager;
        public static RoleManager RoleManager
        {
            get
            {
                if (roleManager == null)
                    roleManager = new RoleManager();
                return roleManager;
            }
        }

    }
}