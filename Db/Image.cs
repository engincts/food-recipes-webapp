using System;
using System.Collections.Generic;

namespace YemekTarifleri.Db;

public partial class Image
{
    public int ImageId { get; set; }

    public byte[]? ImageContent { get; set; }

    public string? FileName { get; set; }

    public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
}
