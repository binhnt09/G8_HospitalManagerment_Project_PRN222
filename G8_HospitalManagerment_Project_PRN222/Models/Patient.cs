using System;
using System.Collections.Generic;

namespace G8_HospitalManagerment_Project_PRN222.Models;

public partial class Patient
{
    public int PatientId { get; set; }

    public string? BloodType { get; set; }

    public string? Allergies { get; set; }

    public string? InsuranceNumber { get; set; }

    public int UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<ImagingOrder> ImagingOrders { get; set; } = new List<ImagingOrder>();

    public virtual ICollection<InpatientAdmission> InpatientAdmissions { get; set; } = new List<InpatientAdmission>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<LabOrder> LabOrders { get; set; } = new List<LabOrder>();

    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public virtual ICollection<SurgerySchedule> SurgerySchedules { get; set; } = new List<SurgerySchedule>();

    public virtual User User { get; set; } = null!;
}
