using System;
using System.Collections.Generic;

namespace YemekTarifleri.Db;

public partial class IngredientUnit
{
    public int UnitId { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
}
