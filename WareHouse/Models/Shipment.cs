using System;
using System.Collections.Generic;

namespace WareHouse.Models;

public partial class Shipment
{
    public string Id { get; set; } = null!;

    public DateOnly Date { get; set; }

    public int? Statusid { get; set; }

    public string? Userid { get; set; }

    public virtual ICollection<Shipmentitem> Shipmentitems { get; set; } = new List<Shipmentitem>();

    public virtual Status? Status { get; set; }

    public virtual User? User { get; set; }
}
