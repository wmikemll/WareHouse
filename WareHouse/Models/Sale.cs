using System;
using System.Collections.Generic;

namespace WareHouse.Models;

public partial class Sale
{
    public string Id { get; set; } = null!;

    public DateOnly Date { get; set; }

    public string? Userid { get; set; }
    public bool? IsHidden { get; set; }
    public int StatusId { get; set; }

    public virtual ICollection<Saleitem> Saleitems { get; set; } = new List<Saleitem>();

    public virtual User? User { get; set; }
    public virtual Status? Status { get; set; }
}
