using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace G8_HospitalManagerment_Project_PRN222.ViewModels
{
    public class Encounter1CreateViewModel
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }

        // Display-only helpers (not posted)
        public string? PatientName { get; set; }
        public string? DoctorName { get; set; }
        public DateTime? AppointmentDate { get; set; }

        // Editable clinical fields
        [Display(Name = "Chẩn đoán")]
        [Required(ErrorMessage = "Vui lòng nhập chẩn đoán.")]
        public string Diagnosis { get; set; } = string.Empty;

        [Display(Name = "Triệu chứng")]
        public string? Symptoms { get; set; }
        [Display(Name = "Phương án điều trị")]
        public string? Treatment { get; set; }

        // Danh sách ID các xét nghiệm & siêu âm/X-quang được chọn
        public List<int> SelectedLabTests { get; set; } = new List<int>();
        public List<int> SelectedImagingServices { get; set; } = new List<int>();

        // Danh sách để render Checkbox/Select2 trên UI
        public List<SelectListItem> AvailableLabTests { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> AvailableImagingServices { get; set; } = new List<SelectListItem>();
    }

    // ==========================================
    // KHÁM LẦN 2: Chốt bệnh, Hướng điều trị & Ra quyết định
    // ==========================================
    public class Encounter2EditViewModel
    {
        public int RecordId { get; set; }
        public string PatientName { get; set; }

        // BÊN TRÁI: Dữ liệu Read-only (Kết quả cận lâm sàng)
        public List<LabResultDTO> LabResults { get; set; } = new List<LabResultDTO>();
        public List<ImagingResultDTO> ImagingResults { get; set; } = new List<ImagingResultDTO>();

        // BÊN PHẢI: Dữ liệu Editable
        [Required(ErrorMessage = "Vui lòng nhập chẩn đoán xác định")]
        [Display(Name = "Chẩn đoán xác định (Final Diagnosis)")]
        public string FinalDiagnosis { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập hướng điều trị")]
        [Display(Name = "Hướng điều trị (Treatment)")]
        public string Treatment { get; set; }

        // Option Quyết định của Bác sĩ (Ngoại trú, Nội trú, Phẫu thuật)
        [Required(ErrorMessage = "Vui lòng chọn hướng xử lý tiếp theo")]
        public string DecisionOption { get; set; }
    }

    public class LabResultDTO
    {
        public string TestName { get; set; }
        public string ResultValue { get; set; }
        public string Status { get; set; }
    }

    public class ImagingResultDTO
    {
        public string ServiceName { get; set; }
        public string Conclusion { get; set; }
        public string ImageUrl { get; set; }
    }
}
