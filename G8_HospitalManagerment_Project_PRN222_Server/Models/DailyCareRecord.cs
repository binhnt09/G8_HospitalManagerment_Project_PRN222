using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace G8_HospitalManagerment_Project_PRN222_Server.Models;

public partial class DailyCareRecord
{
    [Key]
    [Column("CareID")]
    public int CareId { get; set; }

    [Column("AdmissionID")]
    public int? AdmissionId { get; set; }

    [Column("EmployeeID")]
    public int? EmployeeId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? RecordDate { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? Temperature { get; set; }

    [StringLength(20)]
    public string? BloodPressure { get; set; }

    public string? Notes { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [ForeignKey("AdmissionId")]
    [InverseProperty("DailyCareRecords")]
    public virtual InpatientAdmission? Admission { get; set; }

    [ForeignKey("EmployeeId")]
    [InverseProperty("DailyCareRecords")]
    public virtual Employee? Employee { get; set; }
}
