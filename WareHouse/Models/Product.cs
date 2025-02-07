using System;
using System.Collections.Generic;

namespace WareHouse.Models;

public partial class Product
{
    public string Id { get; set; } = null!;

    public decimal Price { get; set; }

    public string Name { get; set; } = null!;

    public int Count { get; set; }

    public int? Categoryid { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<Saleitem> Saleitems { get; set; } = new List<Saleitem>();

    public virtual ICollection<Shipmentitem> Shipmentitems { get; set; } = new List<Shipmentitem>();
}
