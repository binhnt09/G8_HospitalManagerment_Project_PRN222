using System;
using System.Collections.Generic;

namespace G8_HospitalManagerment_Project_PRN222.Models;

public partial class Authentication
{
    public int AuthenticationId { get; set; }

    public int UserId { get; set; }




    public string? Password { get; set; }

    public string? AuthType { get; set; }

    public string? ProviderKey { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
