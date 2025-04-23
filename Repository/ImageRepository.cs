using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YemekTarifleri.Db;
using YemekTarifleri.Models;
using YemekTarifleri.Services;

namespace YemekTarifleri.Repository
{
    public class ImageRepository:ImageManager
    {

        public async Task<byte[]> SaveImageFile(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            using (var memoryStream = new MemoryStream())
            {
                await imageFile.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }


        public byte[] ConvertToByteArray(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }


        //[HttpPost]
        //public async Task<IActionResult> UploadImage(int recipeId, IFormFile file)
        //{
        //    try
        //    {
        //        if (file != null && file.Length > 0)
        //        {
        //            var fileName = Path.GetFileName(file.FileName);
        //            byte[] imgContent;

        //            using (var stream = new MemoryStream())
        //            {
        //                await file.CopyToAsync(stream);
        //                imgContent = stream.ToArray();
        //            }


        //            var imageEntity = new Image
        //            {
        //                ImageContent = imgContent // Save as byte array
        //            };

        //            using (var _context = new YemekTarifleriContext())
        //            {
        //                var recipe = _context.Recipes.Where(R => R.RecipeId == recipeId).FirstOrDefault();
        //                if (recipe == null)
        //                {
        //                    throw new Exception("Recipe is not found");
        //                }




        //                recipe.ImageId = imageEntity.ImageId;
        //                _context.Update(recipe);
        //                await _context.SaveChangesAsync();

        //                return Ok(new { ImageId = imageEntity.ImageId });
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("No file uploaded");
        //        }
        //    }
        //    catch (Exception exc)
        //    {
        //        return BadRequest(exc.Message);
        //    }

        //}
        public List<ImageViewModel> GetAllImages()
        {
            using (var _context = new YemekTarifleriContext())
            {
                var images = _context.Images
                    .Select(img => new ImageViewModel
                    {
                        ImageId = img.ImageId,
                        FileName = img.FileName,
                        ImagePath = $"/images/{img.FileName}", // Assuming images are stored in a folder
                        recipes = img.Recipes.Select(r => new RecipeViewModel
                        {
                            RecipeId = r.RecipeId,
                            RecipeName = r.RecipeName,
                            Rating = r.Rating,
                            Description = r.Description,
                            ImageId = r.ImageId
                        }).ToList()
                    }).ToList();

                return images;
            }
        }








        public int CreateImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                throw new ArgumentException("Image file is required.");
            }

            using (var _context = new YemekTarifleriContext())
            {
                try
                {
                    var imageEntity = new Image
                    {
                        // Save the image file and set the path
                        //ImagePath = SaveImageFile(imageFile), // Use ImagePath if it stores the file path
                                                              // Optionally, you might want to save the file content as well
                        ImageContent = ConvertToByteArray(imageFile)
                    };

                    _context.Images.Add(imageEntity);
                    _context.SaveChanges();

                    return imageEntity.ImageId; // Return the saved ImageId
                }
                catch (Exception ex)
                {
                    // Handle exception (log error, rethrow, etc.)
                    throw new ApplicationException("An error occurred while saving the image.", ex);
                }
            }
        }


    }
}
