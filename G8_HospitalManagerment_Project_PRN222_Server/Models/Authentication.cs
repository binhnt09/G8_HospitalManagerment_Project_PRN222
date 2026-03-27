using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace G8_HospitalManagerment_Project_PRN222_Server.Models;

[Table("Authentication")]
public partial class Authentication
{
    [Key]
    [Column("AuthenticationID")]
    public int AuthenticationId { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [StringLength(255)]
    public string? Password { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? AuthType { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? ProviderKey { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Authentications")]
    public virtual User User { get; set; } = null!;
}
