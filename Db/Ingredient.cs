using System;
using System.Collections.Generic;

namespace YemekTarifleri.Db;

public partial class Ingredient
{
    public int IngredientId { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
}
