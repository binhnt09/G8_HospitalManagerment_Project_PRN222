using System;
using System.Collections.Generic;

namespace G8_HospitalManagerment_Project_PRN222.Models;

public partial class Bed
{
    public int BedId { get; set; }

    public int? RoomId { get; set; }

    public string BedNumber { get; set; } = null!;

    public bool? IsOccupied { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<InpatientAdmission> InpatientAdmissions { get; set; } = new List<InpatientAdmission>();

    public virtual Room? Room { get; set; }
}
