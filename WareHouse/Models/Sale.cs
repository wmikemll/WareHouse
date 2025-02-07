using System;
using System.Collections.Generic;

namespace WareHouse.Models;

public partial class Sale
{
    public string Id { get; set; } = null!;

    public DateOnly Date { get; set; }

    public string? Userid { get; set; }

    public virtual ICollection<Saleitem> Saleitems { get; set; } = new List<Saleitem>();

    public virtual User? User { get; set; }
}
