using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace G8_HospitalManagerment_Project_PRN222_Server.Models;

public partial class ImagingOrder
{
    [Key]
    [Column("OrderID")]
    public int OrderId { get; set; }

    [Column("MedicalRecordID")]
    public int MedicalRecordId { get; set; }

    [Column("PatientID")]
    public int PatientId { get; set; }

    [Column("DoctorID")]
    public int DoctorId { get; set; }

    [Column("ServiceID")]
    public int ServiceId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? OrderDate { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [ForeignKey("DoctorId")]
    [InverseProperty("ImagingOrders")]
    public virtual Doctor Doctor { get; set; } = null!;

    [InverseProperty("Order")]
    public virtual ICollection<ImagingResult> ImagingResults { get; set; } = new List<ImagingResult>();

    [ForeignKey("MedicalRecordId")]
    [InverseProperty("ImagingOrders")]
    public virtual MedicalRecord MedicalRecord { get; set; } = null!;

    [ForeignKey("PatientId")]
    [InverseProperty("ImagingOrders")]
    public virtual Patient Patient { get; set; } = null!;

    [ForeignKey("ServiceId")]
    [InverseProperty("ImagingOrders")]
    public virtual Service Service { get; set; } = null!;
}
