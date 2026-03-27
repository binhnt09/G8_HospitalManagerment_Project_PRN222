using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace G8_HospitalManagerment_Project_PRN222_Server.Models;

public partial class InpatientAdmission
{
    [Key]
    [Column("AdmissionID")]
    public int AdmissionId { get; set; }

    [Column("PatientID")]
    public int? PatientId { get; set; }

    [Column("DoctorID")]
    public int? DoctorId { get; set; }

    [Column("BedID")]
    public int? BedId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? AdmissionDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DischargeDate { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [ForeignKey("BedId")]
    [InverseProperty("InpatientAdmissions")]
    public virtual Bed? Bed { get; set; }

    [InverseProperty("Admission")]
    public virtual ICollection<DailyCareRecord> DailyCareRecords { get; set; } = new List<DailyCareRecord>();

    [ForeignKey("DoctorId")]
    [InverseProperty("InpatientAdmissions")]
    public virtual Doctor? Doctor { get; set; }

    [ForeignKey("PatientId")]
    [InverseProperty("InpatientAdmissions")]
    public virtual Patient? Patient { get; set; }
}
