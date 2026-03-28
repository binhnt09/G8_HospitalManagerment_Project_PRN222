using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace G8_HospitalManagerment_Project_PRN222_Server.Models;

public partial class SurgeryRecord
{
    [Key]
    [Column("RecordID")]
    public int RecordId { get; set; }

    [Column("ScheduleID")]
    public int? ScheduleId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ActualStartTime { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ActualEndTime { get; set; }

    public string? PreOpDiagnosis { get; set; }

    public string? PostOpDiagnosis { get; set; }

    public string? SurgeryMethod { get; set; }

    public string? Notes { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [ForeignKey("ScheduleId")]
    [InverseProperty("SurgeryRecords")]
    public virtual SurgerySchedule? Schedule { get; set; }
}
