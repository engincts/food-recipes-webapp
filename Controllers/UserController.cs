using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YemekTarifleri.Db;
using YemekTarifleri.ViewModels;

namespace YemekTarifleri.Controllers
{
    public class UserController : Controller
    {
        public readonly UserManager<AppUser> _userManager;
        public readonly SignInManager<AppUser> _signInManager;
        public UserController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager) 
        {
            _userManager=userManager;
            _signInManager = signInManager;
        }


    }
}
