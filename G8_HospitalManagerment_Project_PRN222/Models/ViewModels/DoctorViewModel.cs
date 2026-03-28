using System.ComponentModel.DataAnnotations;

namespace G8_HospitalManagerment_Project_PRN222.Models.ViewModels
{
    public class DoctorViewModel
    {
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn chuyên khoa")]
        [Display(Name = "Chuyên khoa")]
        public string? Specialization { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số năm kinh nghiệm")]
        [Range(0, 60, ErrorMessage = "Kinh nghiệm phải từ 0 đến 60 năm")]
        public int? YearsExperience { get; set; }

        [Required(ErrorMessage = "Số chứng chỉ hành nghề không được để trống")]
        [StringLength(50, ErrorMessage = "Số CCHN quá dài")]
        public string? LicenseNumber { get; set; }

        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn phòng ban")]
        public int DepartmentId { get; set; }

        public int UserId { get; set; }

        public string? Position { get; set; }
        public string? FullName { get; set; }
    }
}