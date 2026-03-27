using System;
using System.Collections.Generic;

namespace G8_HospitalManagerment_Project_PRN222.Models;

public partial class OperationRoom
{
    public int OroomId { get; set; }

    public string RoomName { get; set; } = null!;

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<SurgerySchedule> SurgerySchedules { get; set; } = new List<SurgerySchedule>();
}
