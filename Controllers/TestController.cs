using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YemekTarifleri.Db;
using YemekTarifleri.Models;
using YemekTarifleri.Services;
using Microsoft.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace YemekTarifleri.Controllers
{
    //[ApiController]
    //[Route("[controller]")]
   
    public class TestController : Controller
    {
        private readonly IConfiguration _configuration;

        public TestController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }


        //[HttpPost]
        //public IActionResult CreateUser([FromBody]UserLoginModel user)
        //{

        //    if (user == null)
        //    {
        //        return BadRequest("User data is null.");
        //    }

        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest("Invalid model state.");

        //    }
        //    var userCreationResult = Util.user.CreateUser(user);

        //    if (userCreationResult && user.Password == user.PasswordControl)
        //    {
        //        var claims = new List<Claim> {
        //        new Claim(ClaimTypes.Name, user.UserName),
        //        new Claim(ClaimTypes.NameIdentifier, user.Password),

        //        };
        //        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        //        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        //        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

        //        return RedirectToAction("Index", "Home");
        //    }
        //    else
        //    {
        //        TempData["error"] = "User creation failed. Please try again.";
        //        return RedirectToAction("CreateUser");



        //    }
        //}
        [HttpPut("UpdateUser")]
        public IActionResult UpdateUser([FromBody] UserUpdateModel updateModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model state.");
            }

            // Call the service method to update the user
            bool userUpdated = Util.user.UpdateUser(updateModel);

            if (userUpdated)
            {
                return Ok("User updated successfully.");
            }
            else
            {
                return BadRequest("User update failed. User might not exist or passwords do not match.");
            }
        }

        [HttpPost]
        public IActionResult GetUserById([FromBody] UserLoginModel user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model state");
            }

            // Find the user by username and password
            using (YemekTarifleriContext context = new YemekTarifleriContext())
            {
                var existingUser = context.AppUsers
                                          .FirstOrDefault(u => u.UserName == user.UserName && u.Password == user.Password);

                if (existingUser != null)
                {
                    return Ok(new { UserId = existingUser.UserId });
                }
                else
                {
                    return NotFound("User not found");
                }
            }
        }
        [HttpDelete]
        public IActionResult DeleteUser(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("Invalid user ID.");
            }

            bool result = Util.user.DeleteUser(userId); // Adjust method accordingly
            if (!result)
            {
                TempData["ErrorMessage"] = "User not found or deletion failed.";
                return RedirectToAction("Profile", "Home");
            }

            HttpContext.SignOutAsync(); // This removes the authentication cookie

            TempData["SuccessMessage"] = "User successfully deleted.";
            return RedirectToAction("Login");
        }


        [HttpGet]
        public IActionResult GetAllUsers()
        {
            using (var dbContext = new YemekTarifleriContext())
            {
                var users = dbContext.AppUsers.ToList();
                return Ok(users);
            }
        }
        [HttpGet]
        public IActionResult GetAllRecipes()
        {
            List<Recipe> recipes = new List<Recipe>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open(); // Open the connection here
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM Recipe", connection))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var recipe = new Recipe()
                            {
                                RecipeId = reader.GetInt32(0),
                                RecipeName = reader.GetString(1),
                                Rating = reader.GetInt32(6),
                                CookTime = reader.GetInt32(4),
                                Description = reader.GetString(2),
                                Servings = reader.GetInt32(5),
                                PrepTime = reader.GetInt32(3),
                                Steps = reader.GetString(7),
                                //ImageId = reader.GetInt32(8),
                            };

                            // No need to close the connection here
                            //recipe.Image = GetImageById(recipe.ImageId, connection);
                            recipe.RecipeIngredients = GetIngredientsByRecipeId(recipe.RecipeId, connection);
                            recipes.Add(recipe);
                        }
                    }
                }
            }
            return Json(recipes);
        }
        [HttpGet]
        public ICollection<RecipeIngredient> GetIngredientsByRecipeId(int recipeId, SqlConnection connection)
        {
            List<RecipeIngredient> recipeIngredients = new List<RecipeIngredient>();

            string query = @"SELECT r.RecipeIngredientId, r.UnitId, r.IngredientId, r.Amount,
                            i.UnitId, i.Name AS UnitName, 
                            ing.IngredientId, ing.Name AS IngredientName 
                     FROM RecipeIngredient r
                     INNER JOIN IngredientUnit i ON r.UnitId = i.UnitId
                     INNER JOIN Ingredient ing ON r.IngredientId = ing.IngredientId
                     WHERE r.RecipeId = @RecipeId";

            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@RecipeId", recipeId);

                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        var ingredient = new RecipeIngredient()
                        {
                            RecipeIngredientId = r.GetInt32(r.GetOrdinal("RecipeIngredientId")),
                            UnitId = r.GetInt32(r.GetOrdinal("UnitId")),
                            IngredientId = r.GetInt32(r.GetOrdinal("IngredientId")),
                            Amount = r.GetDecimal(r.GetOrdinal("Amount")),

                            Unit = new IngredientUnit
                            {
                                UnitId = r.GetInt32(r.GetOrdinal("UnitId")),
                                Name = r.GetString(r.GetOrdinal("Name")) // Use the alias here
                            },
                            Ingredient = new Ingredient
                            {
                                IngredientId = r.GetInt32(r.GetOrdinal("IngredientId")),
                                Name = r.GetString(r.GetOrdinal("Name")) // Use the alias here
                            }
                        };

                        recipeIngredients.Add(ingredient);
                    }
                }
            }

            return recipeIngredients;
        }
        public void DeleteRecipe(int recipeId, SqlConnection connection)
        {
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    // First, delete related RecipeIngredients
                    string deleteIngredientsQuery = "DELETE FROM RecipeIngredient WHERE RecipeId = @RecipeId";
                    using (SqlCommand cmd = new SqlCommand(deleteIngredientsQuery, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@RecipeId", recipeId);
                        cmd.ExecuteNonQuery();
                    }

                    // Then, delete the recipe
                    string deleteRecipeQuery = "DELETE FROM Recipe WHERE RecipeId = @RecipeId";
                    using (SqlCommand cmd = new SqlCommand(deleteRecipeQuery, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@RecipeId", recipeId);
                        cmd.ExecuteNonQuery();
                    }

                    // Optionally, delete any ingredients that are no longer used by any recipes
                    string deleteUnusedIngredientsQuery = @"
                DELETE FROM Ingredient
                WHERE IngredientId NOT IN (SELECT DISTINCT IngredientId FROM RecipeIngredient)";

                    using (SqlCommand cmd = new SqlCommand(deleteUnusedIngredientsQuery, connection, transaction))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Optionally, delete any units that are no longer used by any ingredients
                    string deleteUnusedUnitsQuery = @"
                DELETE FROM IngredientUnit
                WHERE UnitId NOT IN (SELECT DISTINCT UnitId FROM RecipeIngredient)";

                    using (SqlCommand cmd = new SqlCommand(deleteUnusedUnitsQuery, connection, transaction))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Commit the transaction if all deletions succeed
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    // Rollback the transaction in case of an error
                    transaction.Rollback();
                    throw new Exception("An error occurred while deleting the recipe and its related data.", ex);
                }
            }
        }


        public void UpdateRecipe(Recipe recipe, SqlConnection connection)
        {
            // Start a transaction to ensure all updates are executed together
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    // SQL query to update the recipe
                    string updateRecipeQuery = @"UPDATE Recipe
                                         SET RecipeName = @RecipeName,
                                             Description = @Description,
                                             PrepTime = @PrepTime,
                                             CookTime = @CookTime,
                                             Servings = @Servings,
                                             Rating = @Rating,
                                             Steps = @Steps,
                                             ImageId = @ImageId
                                         WHERE RecipeId = @RecipeId";

                    using (SqlCommand cmd = new SqlCommand(updateRecipeQuery, connection, transaction))
                    {
                        // Add parameters to the command
                        cmd.Parameters.AddWithValue("@RecipeName", recipe.RecipeName ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Description", recipe.Description ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@PrepTime", recipe.PrepTime);
                        cmd.Parameters.AddWithValue("@CookTime", recipe.CookTime);
                        cmd.Parameters.AddWithValue("@Servings", recipe.Servings);
                        cmd.Parameters.AddWithValue("@Rating", recipe.Rating);
                        cmd.Parameters.AddWithValue("@Steps", recipe.Steps ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ImageId", (object)recipe.ImageId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@RecipeId", recipe.RecipeId);

                        // Execute the command
                        cmd.ExecuteNonQuery();
                    }

                    // Update RecipeIngredients using JOINs to get IngredientId and UnitId
                    foreach (var ingredient in recipe.RecipeIngredients)
                    {
                        string updateIngredientQuery = @"
                    UPDATE RecipeIngredient
                    SET 
                        IngredientId = Ingredient.IngredientId,
                        Amount = @Amount,
                        UnitId = Unit.UnitId
                    FROM RecipeIngredient
                    JOIN Ingredient ON Ingredient.Name = @IngredientName
                    JOIN IngredientUnit AS Unit ON Unit.Name = @UnitName
                    WHERE RecipeIngredient.RecipeId = @RecipeId
                      AND RecipeIngredient.RecipeIngredientId = @RecipeIngredientId";

                        using (SqlCommand cmd = new SqlCommand(updateIngredientQuery, connection, transaction))
                        {
                            // Add parameters to the command
                            cmd.Parameters.AddWithValue("@IngredientName", ingredient.Ingredient.Name);
                            cmd.Parameters.AddWithValue("@Amount", ingredient.Amount);
                            cmd.Parameters.AddWithValue("@UnitName", ingredient.Unit.Name);
                            cmd.Parameters.AddWithValue("@RecipeIngredientId", ingredient.RecipeIngredientId);
                            cmd.Parameters.AddWithValue("@RecipeId", recipe.RecipeId);

                            // Execute the command
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Commit the transaction if all commands succeed
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    // Rollback the transaction if any command fails
                    transaction.Rollback();

                    // Log or handle the exception as needed
                    throw new Exception("An error occurred while updating the recipe and its ingredients.", ex);
                }
            }
        }



        public void InsertRecipe(Recipe recipe, SqlConnection connection)
        {
            // Define the SQL query to insert a new recipe
            string query = @"INSERT INTO Recipe (RecipeName, Description, PrepTime, CookTime, Servings, Rating, Steps, ImageId)
                     VALUES (@RecipeName, @Description, @PrepTime, @CookTime, @Servings, @Rating, @Steps, @ImageId)";

            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                // Add parameters to prevent SQL injection and handle special characters
                cmd.Parameters.AddWithValue("@RecipeName", recipe.RecipeName);
                cmd.Parameters.AddWithValue("@Description", recipe.Description);
                cmd.Parameters.AddWithValue("@PrepTime", recipe.PrepTime);
                cmd.Parameters.AddWithValue("@CookTime", recipe.CookTime);
                cmd.Parameters.AddWithValue("@Servings", recipe.Servings);
                cmd.Parameters.AddWithValue("@Rating", recipe.Rating);
                cmd.Parameters.AddWithValue("@Steps", recipe.Steps);
                cmd.Parameters.AddWithValue("@ImageId", (object)recipe.ImageId ?? DBNull.Value); // Handle nullable ImageId

                // Open the connection if it is not already open
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }

                // Execute the command
                cmd.ExecuteNonQuery();
            }
        }




        //public IngredientUnit GetUnitById(int unitId, SqlConnection connection)
        //{
        //    IngredientUnit unit = null;
        //    using (SqlCommand cmd = new SqlCommand("SELECT * FROM IngredientUnit WHERE UnitId=@UnitId", connection))
        //    {
        //        cmd.Parameters.AddWithValue("@UnitId", unitId);
        //        using (SqlDataReader r = cmd.ExecuteReader())
        //        {
        //            if (r.Read())
        //            {

        //                object val = r["UnitId"];
        //                if (DBNull.Value == val)
        //                {

        //                }

        //                unit = new IngredientUnit()
        //                {
        //                    UnitId = r.GetInt32(r.GetOrdinal("UnitId")),
        //                    Name = r.GetString(r.GetOrdinal("Name")),

        //                };
        //            }
        //        }
        //    }
        //    return unit;
        //}

        //public Ingredient GetIngredientById(int ingredientId, SqlConnection connection)
        //{
        //    Ingredient ingredient = null;
        //    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Ingredient WHERE IngredientId=@IngredientId", connection))
        //    {
        //        cmd.Parameters.AddWithValue("@IngredientId", ingredientId);
        //        using (SqlDataReader r = cmd.ExecuteReader())
        //        {
        //            if (r.Read())
        //            {
        //                ingredient = new Ingredient()
        //                {
        //                    IngredientId = r.GetInt32(r.GetOrdinal("IngredientId")),
        //                    Name = r.GetString(r.GetOrdinal("Name")),
        //                };
        //            }
        //        }
        //    }
        //    return ingredient;
        //}

        //public Image GetImageById(int imageId, SqlConnection connection)
        //{
        //    Image image = null;
        //    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Image WHERE ImageId=@ImageId", connection))
        //    {
        //        cmd.Parameters.AddWithValue("@ImageId", imageId);
        //        using (SqlDataReader reader = cmd.ExecuteReader())
        //        {
        //            if (reader.Read())
        //            {
        //                image = new Image()
        //                {
        //                    ImageId = reader.GetInt32(0),
        //                };
        //            }
        //        }
        //    }
        //    return image;

        //}




    }
}
