using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace G8_HospitalManagerment_Project_PRN222_Server.Models;

public partial class SurgerySchedule
{
    [Key]
    [Column("ScheduleID")]
    public int ScheduleId { get; set; }

    [Column("PatientID")]
    public int? PatientId { get; set; }

    [Column("ORoomID")]
    public int? OroomId { get; set; }

    [Column("MainSurgeonID")]
    public int? MainSurgeonId { get; set; }

    [Column("AnesthesiologistID")]
    public int? AnesthesiologistId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PlannedStartTime { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PlannedEndTime { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [ForeignKey("AnesthesiologistId")]
    [InverseProperty("SurgeryScheduleAnesthesiologists")]
    public virtual Doctor? Anesthesiologist { get; set; }

    [ForeignKey("MainSurgeonId")]
    [InverseProperty("SurgeryScheduleMainSurgeons")]
    public virtual Doctor? MainSurgeon { get; set; }

    [ForeignKey("OroomId")]
    [InverseProperty("SurgerySchedules")]
    public virtual OperationRoom? Oroom { get; set; }

    [ForeignKey("PatientId")]
    [InverseProperty("SurgerySchedules")]
    public virtual Patient? Patient { get; set; }

    [InverseProperty("Schedule")]
    public virtual ICollection<SurgeryRecord> SurgeryRecords { get; set; } = new List<SurgeryRecord>();
}
