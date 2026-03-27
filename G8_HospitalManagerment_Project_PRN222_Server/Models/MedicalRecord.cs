using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace G8_HospitalManagerment_Project_PRN222_Server.Models;

public partial class MedicalRecord
{
    [Key]
    [Column("RecordID")]
    public int RecordId { get; set; }

    [Column("AppointmentID")]
    public int? AppointmentId { get; set; }

    [Column("PatientID")]
    public int PatientId { get; set; }

    [Column("DoctorID")]
    public int DoctorId { get; set; }

    public string? Diagnosis { get; set; }

    public string? Symptoms { get; set; }

    public string? Treatment { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? RecordDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [ForeignKey("AppointmentId")]
    [InverseProperty("MedicalRecords")]
    public virtual Appointment? Appointment { get; set; }

    [ForeignKey("DoctorId")]
    [InverseProperty("MedicalRecords")]
    public virtual Doctor Doctor { get; set; } = null!;

    [InverseProperty("MedicalRecord")]
    public virtual ICollection<ImagingOrder> ImagingOrders { get; set; } = new List<ImagingOrder>();

    [InverseProperty("MedicalRecord")]
    public virtual ICollection<LabOrder> LabOrders { get; set; } = new List<LabOrder>();

    [ForeignKey("PatientId")]
    [InverseProperty("MedicalRecords")]
    public virtual Patient Patient { get; set; } = null!;

    [InverseProperty("MedicalRecord")]
    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
