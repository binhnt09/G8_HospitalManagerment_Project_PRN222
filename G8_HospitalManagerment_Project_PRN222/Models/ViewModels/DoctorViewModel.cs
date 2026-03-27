namespace G8_HospitalManagerment_Project_PRN222.Models.ViewModels
{
    public class DoctorViewModel
    {
        public int DoctorId { get; set; }
        public string FullName { get; set; } // Tổng hợp từ Employee.User
        public string? Specialization { get; set; }
        public int? YearsExperience { get; set; }
        public string? LicenseNumber { get; set; }
        public int EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool? IsDeleted { get; set; }
    }
}