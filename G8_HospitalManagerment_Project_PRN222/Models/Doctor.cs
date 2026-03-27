using System;
using System.Collections.Generic;

namespace G8_HospitalManagerment_Project_PRN222.Models;

public partial class Doctor
{
    public int DoctorId { get; set; }

    public string? Specialization { get; set; }

    public int? YearsExperience { get; set; }

    public string? LicenseNumber { get; set; }

    public int EmployeeId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual Employee Employee { get; set; } = null!;

    public virtual ICollection<ImagingOrder> ImagingOrders { get; set; } = new List<ImagingOrder>();

    public virtual ICollection<InpatientAdmission> InpatientAdmissions { get; set; } = new List<InpatientAdmission>();

    public virtual ICollection<LabOrder> LabOrders { get; set; } = new List<LabOrder>();

    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public virtual ICollection<SurgerySchedule> SurgeryScheduleAnesthesiologists { get; set; } = new List<SurgerySchedule>();

    public virtual ICollection<SurgerySchedule> SurgeryScheduleMainSurgeons { get; set; } = new List<SurgerySchedule>();
}
