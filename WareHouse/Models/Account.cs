using System;
using System.Collections.Generic;

namespace WareHouse.Models;

public partial class Account
{
    public string Id { get; set; }
    public string Mail { get; set; } = null!;

    public string? Phone { get; set; }

    public string Password { get; set; } = null!;

    public DateOnly? CreatedDate { get; set; }

    public bool Isactive { get; set; }

    public string? Userid { get; set; }

    public virtual User? User { get; set; }
}
