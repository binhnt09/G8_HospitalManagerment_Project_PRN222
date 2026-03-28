using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace G8_HospitalManagerment_Project_PRN222_Server.Models;

public partial class OperationRoom
{
    [Key]
    [Column("ORoomID")]
    public int OroomId { get; set; }

    [StringLength(100)]
    public string RoomName { get; set; } = null!;

    [StringLength(50)]
    public string? Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [InverseProperty("Oroom")]
    public virtual ICollection<SurgerySchedule> SurgerySchedules { get; set; } = new List<SurgerySchedule>();
}
