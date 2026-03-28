using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace G8_HospitalManagerment_Project_PRN222_Server.Models;

public partial class LabOrderItem
{
    [Key]
    [Column("OrderItemID")]
    public int OrderItemId { get; set; }

    [Column("OrderID")]
    public int OrderId { get; set; }

    [Column("TestID")]
    public int TestId { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [InverseProperty("OrderItem")]
    public virtual ICollection<LabResult> LabResults { get; set; } = new List<LabResult>();

    [ForeignKey("OrderId")]
    [InverseProperty("LabOrderItems")]
    public virtual LabOrder Order { get; set; } = null!;

    [ForeignKey("TestId")]
    [InverseProperty("LabOrderItems")]
    public virtual Test Test { get; set; } = null!;
}
