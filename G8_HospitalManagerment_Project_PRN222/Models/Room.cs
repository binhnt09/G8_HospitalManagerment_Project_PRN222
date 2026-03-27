using System;
using System.Collections.Generic;

namespace G8_HospitalManagerment_Project_PRN222.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public string RoomName { get; set; } = null!;

    public string? RoomType { get; set; }

    public decimal PricePerDay { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<Bed> Beds { get; set; } = new List<Bed>();
}
