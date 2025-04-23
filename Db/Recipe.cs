using System;
using System.Collections.Generic;

namespace YemekTarifleri.Db;

public partial class Recipe
{
    public int RecipeId { get; set; }

    public string? RecipeName { get; set; }

    public string? Description { get; set; }

    public int? PrepTime { get; set; }

    public int? CookTime { get; set; }

    public int? Servings { get; set; }

    public int? Rating { get; set; }

    public string? Steps { get; set; }

    public int ImageId { get; set; }

    public virtual Image Image { get; set; } = null!;

    public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
}
