using System;
using System.Collections.Generic;

namespace WareHouse.Models;

public partial class User
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string? Patronomic { get; set; }

    public int? Roleid { get; set; }

    public virtual Account? Account { get; set; }

    public virtual Role? Role { get; set; }

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
}
