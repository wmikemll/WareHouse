using System;
using System.Collections.Generic;

namespace WareHouse.Models;

public partial class Saleitem
{
    public string Id { get; set; } = null!;

    public string Productid { get; set; } = null!;

    public int Count { get; set; }

    public string Saleid { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual Sale Sale { get; set; } = null!;
}
