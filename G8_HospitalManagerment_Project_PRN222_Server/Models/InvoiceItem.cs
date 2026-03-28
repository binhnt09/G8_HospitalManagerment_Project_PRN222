using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace G8_HospitalManagerment_Project_PRN222_Server.Models;

public partial class InvoiceItem
{
    [Key]
    [Column("InvoiceItemID")]
    public int InvoiceItemId { get; set; }

    [Column("InvoiceID")]
    public int InvoiceId { get; set; }

    [StringLength(50)]
    public string? ItemType { get; set; }

    [Column("ReferenceID")]
    public int ReferenceId { get; set; }

    [StringLength(255)]
    public string ItemName { get; set; } = null!;

    public int? Quantity { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal TotalPrice { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [ForeignKey("InvoiceId")]
    [InverseProperty("InvoiceItems")]
    public virtual Invoice Invoice { get; set; } = null!;
}
