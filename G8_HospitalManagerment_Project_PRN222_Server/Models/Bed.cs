using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace G8_HospitalManagerment_Project_PRN222_Server.Models;

public partial class Bed
{
    [Key]
    [Column("BedID")]
    public int BedId { get; set; }

    [Column("RoomID")]
    public int? RoomId { get; set; }

    [StringLength(20)]
    public string BedNumber { get; set; } = null!;

    public bool? IsOccupied { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [InverseProperty("Bed")]
    public virtual ICollection<InpatientAdmission> InpatientAdmissions { get; set; } = new List<InpatientAdmission>();

    [ForeignKey("RoomId")]
    [InverseProperty("Beds")]
    public virtual Room? Room { get; set; }
}
