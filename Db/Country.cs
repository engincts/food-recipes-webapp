using System;
using System.Collections.Generic;

namespace YemekTarifleri.Db;

public partial class Country
{
    public string? CountryName { get; set; }

    public int CountryId { get; set; }

    public int? UserId { get; set; }

    public int? RecipeId { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public string? City { get; set; }
}
