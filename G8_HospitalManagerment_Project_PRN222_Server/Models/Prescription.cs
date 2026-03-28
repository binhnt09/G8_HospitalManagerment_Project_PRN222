using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace G8_HospitalManagerment_Project_PRN222_Server.Models;

public partial class Prescription
{
    [Key]
    [Column("PrescriptionID")]
    public int PrescriptionId { get; set; }

    [Column("MedicalRecordID")]
    public int MedicalRecordId { get; set; }

    [Column("PatientID")]
    public int PatientId { get; set; }

    [Column("DoctorID")]
    public int DoctorId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PrescriptionDate { get; set; }

    [StringLength(255)]
    public string? DoctorAdvice { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [ForeignKey("DoctorId")]
    [InverseProperty("Prescriptions")]
    public virtual Doctor Doctor { get; set; } = null!;

    [ForeignKey("MedicalRecordId")]
    [InverseProperty("Prescriptions")]
    public virtual MedicalRecord MedicalRecord { get; set; } = null!;

    [ForeignKey("PatientId")]
    [InverseProperty("Prescriptions")]
    public virtual Patient Patient { get; set; } = null!;

    [InverseProperty("Prescription")]
    public virtual ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
}
