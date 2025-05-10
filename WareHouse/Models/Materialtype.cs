using System;
using System.Collections.Generic;

namespace WareHouse.Models;

public partial class Materialtype
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool IsDeleted { get; set; } = false;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
