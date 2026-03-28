using System;
using System.Collections.Generic;

namespace G8_HospitalManagerment_Project_PRN222.Models;

public partial class ImagingResult
{
    public int ResultId { get; set; }

    public int OrderId { get; set; }

    public string? Description { get; set; }

    public string? Conclusion { get; set; }

    public string? ImageUrls { get; set; }

    public int? PerformedBy { get; set; }

    public DateTime? ResultDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ImagingOrder Order { get; set; } = null!;

    public virtual Employee? PerformedByNavigation { get; set; }
}
