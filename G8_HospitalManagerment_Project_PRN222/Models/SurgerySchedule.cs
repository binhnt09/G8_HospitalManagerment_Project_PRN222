using System;
using System.Collections.Generic;

namespace G8_HospitalManagerment_Project_PRN222.Models;

public partial class SurgerySchedule
{
    public int ScheduleId { get; set; }

    public int? PatientId { get; set; }

    public int? OroomId { get; set; }

    public int? MainSurgeonId { get; set; }

    public int? AnesthesiologistId { get; set; }

    public DateTime? PlannedStartTime { get; set; }

    public DateTime? PlannedEndTime { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Doctor? Anesthesiologist { get; set; }

    public virtual Doctor? MainSurgeon { get; set; }

    public virtual OperationRoom? Oroom { get; set; }

    public virtual Patient? Patient { get; set; }

    public virtual ICollection<SurgeryRecord> SurgeryRecords { get; set; } = new List<SurgeryRecord>();
}
