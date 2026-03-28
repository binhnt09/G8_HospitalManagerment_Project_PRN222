using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace G8_HospitalManagerment_Project_PRN222_Server.Models;

[Index("EmployeeCode", Name = "UQ__Employee__1F642548DE995172", IsUnique = true)]
public partial class Employee
{
    [Key]
    [Column("EmployeeID")]
    public int EmployeeId { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string EmployeeCode { get; set; } = null!;

    [StringLength(100)]
    public string? Position { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? WorkStatus { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? HireDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? TerminationDate { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [Column("DepartmentID")]
    public int DepartmentId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [InverseProperty("Employee")]
    public virtual ICollection<DailyCareRecord> DailyCareRecords { get; set; } = new List<DailyCareRecord>();

    [ForeignKey("DepartmentId")]
    [InverseProperty("Employees")]
    public virtual Department Department { get; set; } = null!;

    [InverseProperty("Employee")]
    public virtual Doctor? Doctor { get; set; }

    [InverseProperty("PerformedByNavigation")]
    public virtual ICollection<ImagingResult> ImagingResults { get; set; } = new List<ImagingResult>();

    [InverseProperty("PerformedByNavigation")]
    public virtual ICollection<LabResult> LabResults { get; set; } = new List<LabResult>();

    [ForeignKey("UserId")]
    [InverseProperty("Employees")]
    public virtual User User { get; set; } = null!;
}
