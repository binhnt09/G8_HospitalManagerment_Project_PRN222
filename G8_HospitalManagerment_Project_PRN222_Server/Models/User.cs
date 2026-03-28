using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace G8_HospitalManagerment_Project_PRN222_Server.Models;

[Table("User")]
public partial class User
{
    [Key]
    [Column("UserID")]
    public int UserId { get; set; }

    [StringLength(100)]
    public string FirstName { get; set; } = null!;

    [StringLength(100)]
    public string LastName { get; set; } = null!;

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(10)]
    public string? Gender { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? BirthDay { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    public bool? Verified { get; set; }

    [Column("UserRoleID")]
    public int UserRoleId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Authentication> Authentications { get; set; } = new List<Authentication>();

    [InverseProperty("User")]
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    [InverseProperty("User")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [InverseProperty("User")]
    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();

    [ForeignKey("UserRoleId")]
    [InverseProperty("Users")]
    public virtual UserRole UserRole { get; set; } = null!;
}
