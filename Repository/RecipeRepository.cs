using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using YemekTarifleri.Db; // Assuming this namespace contains your DbContext
using YemekTarifleri.Models;
using YemekTarifleri.Services;
using static System.Net.Mime.MediaTypeNames;

namespace YemekTarifleri.Repository
{
    public class RecipeRepository : IRecipeManager
    {

        public bool Create(RecipeViewModel model)
        {
            using (var _context = new YemekTarifleriContext())
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // Save Images
                        if (model.ImageFile != null)
                        {
                            using (var stream = new MemoryStream())
                            {
                                // Copy the image file to the memory stream
                                model.ImageFile.CopyTo(stream);

                                var imageEntity = new Db.Image
                                {
                                    ImageContent = stream.ToArray(), // Save as byte array
                                    FileName = model.ImageFile.FileName
                                };

                                _context.Images.Add(imageEntity);   
                                _context.SaveChanges();

                                model.ImageId = imageEntity.ImageId;
                            }
                        }

                        // Create Recipe Entity
                        var recipeEntity = new Recipe
                        {
                            RecipeName = model.RecipeName,
                            Description = model.Description,
                            PrepTime = model.PrepTime,
                            CookTime = model.CookTime,
                            Servings = model.Servings,
                            Rating = model.Rating,
                            Steps = model.Steps,
                            ImageId = model.ImageId
                        };

                        _context.Recipes.Add(recipeEntity);
                        _context.SaveChanges();

                        // Create RecipeIngredient Entities
                        foreach (var recipeIngredient in model.Ingredients)
                        {
                            var recipeIngredientEntity = new RecipeIngredient
                            {
                                RecipeId = recipeEntity.RecipeId,
                                Amount = recipeIngredient.Amount,
                                IngredientId = recipeIngredient.IngredientId,
                                UnitId = recipeIngredient.UnitId
                            };

                            _context.RecipeIngredients.Add(recipeIngredientEntity);
                        }

                        _context.SaveChanges();

                        // Commit transaction
                        transaction.Commit();
                        return true; // Successfully created
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions, log errors, etc.
                        // Rollback transaction in case of error
                        transaction.Rollback();
                        // Log exception details (ex)
                        return false;
                    }
                }
            }
        }

        public RecipeIngredient UpdateIngredient(IngredientViewModel model)
        {
            using (var context = new YemekTarifleriContext())
            {
                // Include Ingredient and Unit entities
                var recipeIngredient = context.RecipeIngredients
                    .Include(r => r.Ingredient)
                    .Include(r => r.Unit)
                    .FirstOrDefault(r => r.RecipeIngredientId == model.RecipeIngredientId);

                if (recipeIngredient == null)
                {
                    throw new Exception("Ingredient not found");
                }

                // Update properties
                recipeIngredient.Ingredient.Name = recipeIngredient.Ingredient.Name;
                recipeIngredient.Amount = recipeIngredient.Amount;

                // Fetch or create the Unit if necessary
                var unitEntity = context.IngredientUnits.FirstOrDefault(u => u.Name == model.Unit);
                if (unitEntity == null)
                {
                    unitEntity = new IngredientUnit { Name = unitEntity.Name };
                    context.IngredientUnits.Add(unitEntity);
                }
                recipeIngredient.Unit = unitEntity;

                // Mark as modified
                context.Entry(recipeIngredient).State = EntityState.Modified;

                // Save changes
                context.SaveChanges();

                return recipeIngredient;
            }
        }



        public RecipeIngredient AddIngredient(IngredientViewModel model)
        {
            using (var context = new YemekTarifleriContext())
            {
                var recipe = context.Recipes
                    .Include(r => r.RecipeIngredients)
                        .ThenInclude(ri => ri.Ingredient)
                    .Include(r => r.RecipeIngredients)
                        .ThenInclude(ri => ri.Unit)
                    .FirstOrDefault(r => r.RecipeId == model.RecipeId);

                if (recipe == null)
                {
                    throw new Exception("Recipe not found");
                }

                var ingredient = context.Ingredients.FirstOrDefault(i => i.IngredientId == model.IngredientId);
                if (ingredient == null)
                {
                    throw new Exception("Ingredient not found");
                }

                var unit = context.IngredientUnits.FirstOrDefault(u => u.UnitId == model.UnitId);
                if (unit == null)
                {
                    throw new Exception("Unit not found");
                }

                var newRecipeIngredient = new RecipeIngredient
                {
                    IngredientId = ingredient.IngredientId,
                    Amount = model.Amount,
                    UnitId = unit.UnitId,
                    RecipeId = model.RecipeId
                };

                context.RecipeIngredients.Add(newRecipeIngredient);
                context.SaveChanges();

                return newRecipeIngredient;
            }
        }






        public List<Recipe> MenuList()
        {
            using (var context = new YemekTarifleriContext())
            {
                List<Recipe> menuList = context.Recipes
          .Include(r => r.RecipeIngredients)
              .ThenInclude(ri => ri.Ingredient)
          .Include(r => r.RecipeIngredients)
              .ThenInclude(ri => ri.Unit)
              .Include(r => r.Image)
          .ToList();

                return menuList;
            }

        }
        public bool Delete(int recipeId)//Delete Recipe
        {
            using (var context = new YemekTarifleriContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var recipe = context.Recipes
                  .Include(r => r.Image) // Eager load the Image
                  .FirstOrDefault(r => r.RecipeId == recipeId);
                        // Find the Recipe based on RecipeId

                        if (recipe.Image != null)
                        {
                            context.Images.Remove(recipe.Image);
                        }

                        if (recipe != null )
                        {
                            // Remove related Ingredient and IngredientUnit
                            var recipeIngredients = context.RecipeIngredients.Where(ri => ri.RecipeId == recipeId).ToList();
          
                            
                            context.RecipeIngredients.RemoveRange(recipeIngredients);

                            context.Recipes.Remove(recipe);

                            // Save changes to the database
                            context.SaveChanges();

                            // Commit the transaction
                            transaction.Commit();
                        }
 
                    }
                    catch (Exception)
                    {
                        // In case of an error, rollback the transaction
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            return true;
        }

        public bool DeleteIngredient(int recipeIngredientId) // Delete Ingredient
        {
            using (var context = new YemekTarifleriContext())
            {
                // Find the RecipeIngredient based on IngredientId
                var recipeIngredient = context.RecipeIngredients
                    .Include(ri => ri.Ingredient)
                    .Include(ri => ri.Unit)
                    .FirstOrDefault(i => i.RecipeIngredientId == recipeIngredientId);

                if (recipeIngredient != null)
                {
                    // Remove the RecipeIngredient
                    context.RecipeIngredients.Remove(recipeIngredient);

                    // Check if the ingredient is used in any other RecipeIngredients
                    bool ingredientInUse = context.RecipeIngredients.Any(ri => ri.IngredientId == recipeIngredient.IngredientId);

                    // Optionally, remove the associated Ingredient if it's no longer needed
                    if (!ingredientInUse && recipeIngredient.Ingredient != null)
                    {
                        context.Ingredients.Remove(recipeIngredient.Ingredient);
                    }

                    //Check if the unit is used in any other RecipeIngredients
                    bool unitInUse = context.RecipeIngredients.Any(ri => ri.UnitId == recipeIngredient.UnitId);

                    //Optionally, remove the associated Unit if it's no longer needed
                    if (!unitInUse && recipeIngredient.Unit != null)
                    {
                        context.IngredientUnits.Remove(recipeIngredient.Unit);
                    }

                    // Save changes to the database
                    context.SaveChanges();
                    return true; 
                }

                return false; // Return false if RecipeIngredient with given IngredientId was not found
            }
        }


        public List<IngredientViewModel> GetIngredients(int recipeId)
        {
            using (var context = new YemekTarifleriContext())
            {
                // Get the list of RecipeIngredient entities associated with the given RecipeId

                var result = context.RecipeIngredients
             .Join(context.Ingredients,
                 recipeIngredient => recipeIngredient.IngredientId,
                 ingredient => ingredient.IngredientId,
                 (recipeIngredient, ingredient) => new { recipeIngredient, ingredient })
             .Join(context.IngredientUnits,
                 data => data.recipeIngredient.UnitId,
                 ingredientUnit => ingredientUnit.UnitId,
                 (combined, ingredientUnit) => new IngredientViewModel
                 {
                     Amount = combined.recipeIngredient.Amount,
                     IngredientId = combined.recipeIngredient.IngredientId,
                     Name = combined.ingredient.Name,
                     RecipeIngredientId = combined.recipeIngredient.RecipeIngredientId,
                     RecipeId = combined.recipeIngredient.RecipeId,
                     UnitId = combined.recipeIngredient.UnitId,
                     Unit = ingredientUnit.Name // Assuming IngredientUnit has a Name property
                 })
                     .Where(ri => ri.RecipeId == recipeId)
                    .ToList();
                return result;
            }
        }
        [HttpPost]
        public void UploadImage(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                     file.CopyToAsync(memoryStream);
                    var imageData = memoryStream.ToArray();

                    // Save imageData to database or handle accordingly
                }
            }

    
        }
        public List<Db.Image> GetImage(int imageId)
        {
            using (var context = new YemekTarifleriContext())
            {
                var image = context.Images
                    .Where(img => img.ImageId == imageId)
                    .ToList();

                return image;
            }
        }


        public List<Models.ListItem> GetIngredientsList()
        {
            using (var context = new YemekTarifleriContext())
            {
                // Get the list of RecipeIngredient entities associated with the given RecipeId

                return context.Ingredients.Select(D => new ListItem() { Value = D.IngredientId, Text = D.Name })
                    .OrderBy(D => D.Text)
                    .ToList();
            }
        }
        public List<Recipe> GetRecipes(string searchName)
        {
            using (var context = new YemekTarifleriContext())
            {
                //var recipes = context.Recipes;
                //if(searchName==)
                // Fetch all recipes with the specified searchName
                var recipe = context.Recipes
                             .Where(i => i.RecipeName.Contains(searchName))
                             .ToList();

                var list = recipe.Count;


                if (recipe.Count > 0)
                {
                    return recipe;
                }
                else
                {
                    return null;
                }
            }
        }

        public List<RecipeViewModel> GetRecipesByModel()
        {
            var recipes = this.MenuList();

            var list = recipes.Select(recipe => new RecipeViewModel
            {
                RecipeId = recipe.RecipeId,
                ImageId = recipe.ImageId,
                RecipeName = recipe.RecipeName,
                Description = recipe.Description,
                PrepTime = recipe.PrepTime,
                CookTime = recipe.CookTime,
                Servings = recipe.Servings,
                Rating = recipe.Rating,
                TotalTime = (recipe.PrepTime ?? 0) + (recipe.CookTime ?? 0),
                Steps = recipe.Steps,
                Ingredients = recipe.RecipeIngredients.Select(i => new IngredientViewModel
                {
                    RecipeIngredientId = i.RecipeIngredientId,
                    Name = i.Ingredient.Name,
                    Amount = i.Amount,
                    Unit = i.Unit?.Name,
                }).ToList()
            }).ToList();

            return list.OrderByDescending(R => R.Rating).ToList();
        }

    }
}
