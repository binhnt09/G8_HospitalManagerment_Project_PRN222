using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace G8_HospitalManagerment_Project_PRN222_Server.Models;

public partial class Test
{
    [Key]
    [Column("TestID")]
    public int TestId { get; set; }

    [StringLength(100)]
    public string TestName { get; set; } = null!;

    [StringLength(100)]
    public string? Category { get; set; }

    [StringLength(255)]
    public string? ReferenceRange { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? Cost { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [InverseProperty("Test")]
    public virtual ICollection<LabOrderItem> LabOrderItems { get; set; } = new List<LabOrderItem>();
}
