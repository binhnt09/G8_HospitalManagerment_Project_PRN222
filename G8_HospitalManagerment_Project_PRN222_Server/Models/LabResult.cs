using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace G8_HospitalManagerment_Project_PRN222_Server.Models;

public partial class LabResult
{
    [Key]
    [Column("ResultID")]
    public int ResultId { get; set; }

    [Column("OrderItemID")]
    public int OrderItemId { get; set; }

    [StringLength(255)]
    public string? ResultValue { get; set; }

    public bool? IsAbnormal { get; set; }

    [StringLength(255)]
    public string? Remarks { get; set; }

    public int? PerformedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ResultDate { get; set; }

    [ForeignKey("OrderItemId")]
    [InverseProperty("LabResults")]
    public virtual LabOrderItem OrderItem { get; set; } = null!;

    [ForeignKey("PerformedBy")]
    [InverseProperty("LabResults")]
    public virtual Employee? PerformedByNavigation { get; set; }
}
