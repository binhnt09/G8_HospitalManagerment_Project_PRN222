using System;
using System.Collections.Generic;

namespace G8_HospitalManagerment_Project_PRN222.Models;

public partial class SurgeryRecord
{
    public int RecordId { get; set; }

    public int? ScheduleId { get; set; }

    public DateTime? ActualStartTime { get; set; }

    public DateTime? ActualEndTime { get; set; }

    public string? PreOpDiagnosis { get; set; }

    public string? PostOpDiagnosis { get; set; }

    public string? SurgeryMethod { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual SurgerySchedule? Schedule { get; set; }
}
