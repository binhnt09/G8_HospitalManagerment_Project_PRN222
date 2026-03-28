namespace G8_HospitalManagerment_Project_PRN222_Server.Models.ViewModels
{
    public class DoctorViewModel
    {
        // Thông tin Doctor
        public int DoctorId { get; set; }
        public string? Specialization { get; set; }
        public int? YearsExperience { get; set; }
        public string? LicenseNumber { get; set; }

        // Thông tin Employee
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = null!;
        public int DepartmentId { get; set; }
        public int UserId { get; set; }
        public string? Position { get; set; }

        // Thông tin hiển thị (ReadOnly)
        public string? FullName { get; set; }
    }
}