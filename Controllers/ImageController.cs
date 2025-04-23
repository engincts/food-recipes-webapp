using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using YemekTarifleri.Services;
using System.Threading.Tasks;

namespace YemekTarifleri.Controllers
{
    public class ImageController : Controller
    {
        private readonly Services.ImageManager _imageManager;

        public ImageController(ImageManager imageManager)
        {
            _imageManager = imageManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Gallery()
        {
            var images = _imageManager.GetAllImages(); // Tüm görselleri getiren bir metod çağrısı

            if (images == null || !images.Any())
            {
                return NotFound("No images found.");
            }

            return View(images);
        }
    }
}
