using System;
using System.Collections.Generic;

namespace WareHouse.Models;

public partial class Shipmentitem
{
    public string Id { get; set; } = null!;

    public string Productid { get; set; } = null!;

    public double Weight { get; set; }

    public string Shipmentid { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual Shipment Shipment { get; set; } = null!;
}
