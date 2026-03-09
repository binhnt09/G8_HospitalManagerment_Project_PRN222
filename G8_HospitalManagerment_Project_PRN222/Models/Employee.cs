using System;
using System.Collections.Generic;

namespace G8_HospitalManagerment_Project_PRN222.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string EmployeeCode { get; set; } = null!;

    public string? Position { get; set; }

    public string? WorkStatus { get; set; }

    public DateTime? HireDate { get; set; }

    public DateTime? TerminationDate { get; set; }

    public int UserId { get; set; }

    public int DepartmentId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual Doctor? Doctor { get; set; }

    public virtual ICollection<LabResult> LabResults { get; set; } = new List<LabResult>();

    public virtual User User { get; set; } = null!;
}
