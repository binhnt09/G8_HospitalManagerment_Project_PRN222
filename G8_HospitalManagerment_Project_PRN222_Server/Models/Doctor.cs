using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace G8_HospitalManagerment_Project_PRN222_Server.Models;

[Index("EmployeeId", Name = "UQ__Doctors__7AD04FF063847493", IsUnique = true)]
public partial class Doctor
{
    [Key]
    [Column("DoctorID")]
    public int DoctorId { get; set; }

    [StringLength(100)]
    public string? Specialization { get; set; }

    public int? YearsExperience { get; set; }

    [StringLength(100)]
    public string? LicenseNumber { get; set; }

    [Column("EmployeeID")]
    public int EmployeeId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [InverseProperty("Doctor")]
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    [ForeignKey("EmployeeId")]
    [InverseProperty("Doctor")]
    public virtual Employee Employee { get; set; } = null!;

    [InverseProperty("Doctor")]
    public virtual ICollection<ImagingOrder> ImagingOrders { get; set; } = new List<ImagingOrder>();

    [InverseProperty("Doctor")]
    public virtual ICollection<InpatientAdmission> InpatientAdmissions { get; set; } = new List<InpatientAdmission>();

    [InverseProperty("Doctor")]
    public virtual ICollection<LabOrder> LabOrders { get; set; } = new List<LabOrder>();

    [InverseProperty("Doctor")]
    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();

    [InverseProperty("Doctor")]
    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    [InverseProperty("Anesthesiologist")]
    public virtual ICollection<SurgerySchedule> SurgeryScheduleAnesthesiologists { get; set; } = new List<SurgerySchedule>();

    [InverseProperty("MainSurgeon")]
    public virtual ICollection<SurgerySchedule> SurgeryScheduleMainSurgeons { get; set; } = new List<SurgerySchedule>();
}
