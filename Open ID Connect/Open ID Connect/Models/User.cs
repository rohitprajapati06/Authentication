using System;
using System.Collections.Generic;

namespace Open_ID_Connect.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LasttName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? ProfilePhoto { get; set; }

    public string? Id { get; set; }
}
