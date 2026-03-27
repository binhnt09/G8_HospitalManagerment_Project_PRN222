using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace G8_HospitalManagerment_Project_PRN222_Server.Models;

public partial class PrescriptionItem
{
    [Key]
    [Column("PrescriptionItemID")]
    public int PrescriptionItemId { get; set; }

    [Column("PrescriptionID")]
    public int PrescriptionId { get; set; }

    [Column("DrugID")]
    public int DrugId { get; set; }

    public int? Quantity { get; set; }

    [StringLength(100)]
    public string? Dosage { get; set; }

    public int? DurationDays { get; set; }

    [StringLength(255)]
    public string? UsageInstructions { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [ForeignKey("DrugId")]
    [InverseProperty("PrescriptionItems")]
    public virtual Drug Drug { get; set; } = null!;

    [ForeignKey("PrescriptionId")]
    [InverseProperty("PrescriptionItems")]
    public virtual Prescription Prescription { get; set; } = null!;
}
