using System;
using System.Collections.Generic;

namespace WareHouse.Models;

public partial class Product
{
    public string Id { get; set; } = null!;

    public decimal Price { get; set; }

    public string Name { get; set; } = null!;
    public bool isHidden { get; set; } = false!;    
    public int? MaterialTypeId { get; set; }
    public string MaterialBrand { get; set; }

    public double? Weight { get; set; } = 0;
    public double? SpecificGravity { get; set; } = 0;

    public virtual Materialtype? Materialtype { get; set; }

    public virtual ICollection<Saleitem> Saleitems { get; set; } = new List<Saleitem>();

    public virtual ICollection<Shipmentitem> Shipmentitems { get; set; } = new List<Shipmentitem>();
}
