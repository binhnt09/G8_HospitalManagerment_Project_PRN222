using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace G8_HospitalManagerment_Project_PRN222_Server.Models;

public partial class ImagingResult
{
    [Key]
    [Column("ResultID")]
    public int ResultId { get; set; }

    [Column("OrderID")]
    public int OrderId { get; set; }

    public string? Description { get; set; }

    public string? Conclusion { get; set; }

    public string? ImageUrls { get; set; }

    public int? PerformedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ResultDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("ImagingResults")]
    public virtual ImagingOrder Order { get; set; } = null!;

    [ForeignKey("PerformedBy")]
    [InverseProperty("ImagingResults")]
    public virtual Employee? PerformedByNavigation { get; set; }
}
