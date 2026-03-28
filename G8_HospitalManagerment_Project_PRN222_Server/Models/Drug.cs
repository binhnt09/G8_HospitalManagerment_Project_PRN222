using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace G8_HospitalManagerment_Project_PRN222_Server.Models;

public partial class Drug
{
    [Key]
    [Column("DrugID")]
    public int DrugId { get; set; }

    [StringLength(100)]
    public string DrugName { get; set; } = null!;

    [StringLength(50)]
    public string? Unit { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? Price { get; set; }

    public int? StockQuantity { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [InverseProperty("Drug")]
    public virtual ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
}
