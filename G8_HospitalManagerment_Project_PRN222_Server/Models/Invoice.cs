using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace G8_HospitalManagerment_Project_PRN222_Server.Models;

public partial class Invoice
{
    [Key]
    [Column("InvoiceID")]
    public int InvoiceId { get; set; }

    [Column("PatientID")]
    public int PatientId { get; set; }

    [Column("AppointmentID")]
    public int? AppointmentId { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? Discount { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal FinalAmount { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? IssueDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [ForeignKey("AppointmentId")]
    [InverseProperty("Invoices")]
    public virtual Appointment? Appointment { get; set; }

    [InverseProperty("Invoice")]
    public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();

    [ForeignKey("PatientId")]
    [InverseProperty("Invoices")]
    public virtual Patient Patient { get; set; } = null!;

    [InverseProperty("Invoice")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
