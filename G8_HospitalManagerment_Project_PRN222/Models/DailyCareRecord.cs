using System;
using System.Collections.Generic;

namespace G8_HospitalManagerment_Project_PRN222.Models;

public partial class DailyCareRecord
{
    public int CareId { get; set; }

    public int? AdmissionId { get; set; }

    public int? EmployeeId { get; set; }

    public DateTime? RecordDate { get; set; }

    public decimal? Temperature { get; set; }

    public string? BloodPressure { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual InpatientAdmission? Admission { get; set; }

    public virtual Employee? Employee { get; set; }
}
