using System;
using System.Collections.Generic;

namespace YemekTarifleri.Db;

public partial class RecipeIngredient
{
    public decimal? Amount { get; set; }

    public int IngredientId { get; set; }

    public int RecipeId { get; set; }

    public int RecipeIngredientId { get; set; }

    public int UnitId { get; set; }

    public virtual Ingredient Ingredient { get; set; } = null!;

    public virtual Recipe Recipe { get; set; } = null!;

    public virtual IngredientUnit Unit { get; set; } = null!;
}
