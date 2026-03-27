using System.ComponentModel.DataAnnotations;

namespace G8_HospitalManagerment_Project_PRN222.ViewModels
{
    // ─────────────────────────────────────────────────────────────────────────
    // Feature 1 — Xem chi tiết hồ sơ bệnh án (Details view model)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Drug row inside a Prescription shown on the Details page.</summary>
    public class DrugDetailRow
    {
        public string DrugName { get; set; } = string.Empty;
        public string? Dosage { get; set; }
        public int? Quantity { get; set; }
        public string? Unit { get; set; }
        public int? DurationDays { get; set; }
        public string? UsageInstructions { get; set; }
    }

    /// <summary>One Prescription block shown on the Details page.</summary>
    public class PrescriptionDetailBlock
    {
        public int PrescriptionId { get; set; }
        public DateTime? PrescriptionDate { get; set; }
        public string? DoctorAdvice { get; set; }
        public List<DrugDetailRow> Drugs { get; set; } = new();
    }

    /// <summary>Full view model passed to MedicalRecords/Details.cshtml.</summary>
    public class MedicalRecordDetailsViewModel
    {
        public int RecordId { get; set; }
        public DateTime? RecordDate { get; set; }

        // Doctor info
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string? DoctorSpecialization { get; set; }

        // Patient info
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;

        // Clinical data
        public string? Diagnosis { get; set; }
        public string? Symptoms { get; set; }
        public string? Treatment { get; set; }

        public List<PrescriptionDetailBlock> Prescriptions { get; set; } = new();

        public string Status { get; set; } // Để biết là nội trú, mổ hay kê đơn
        public List<LabOrderDetailBlock> LabOrders { get; set; } = new List<LabOrderDetailBlock>();
        public List<ImagingOrderDetailBlock> ImagingOrders { get; set; } = new List<ImagingOrderDetailBlock>();
    }

    public class LabOrderDetailBlock
    {
        public int OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public string Status { get; set; }
        public List<string> TestNames { get; set; } = new List<string>();
    }

    public class ImagingOrderDetailBlock
    {
        public int OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public string Status { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } // Nếu DB bạn kết nối được bảng Service
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Feature 2 — Thêm hồ sơ bệnh án (Create Medical Record form)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Form model bound to MedicalRecords/Create (GET + POST).</summary>
    public class MedicalRecordCreateViewModel
    {
        // Hidden fields — pre-filled from route/query string
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
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Feature 3 — Kê đơn thuốc điện tử (Create Prescription form)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Single drug row inside the prescription form.</summary>
    public class PrescriptionItemViewModel
    {
        [Required]
        public int DrugId { get; set; }

        [Display(Name = "Tên thuốc")]
        public string? DrugName { get; set; }   // display only

        [Required(ErrorMessage = "Vui lòng nhập số lượng.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        [Display(Name = "Liều dùng")]
        public string? Dosage { get; set; }

        [Display(Name = "Số ngày dùng")]
        public int? DurationDays { get; set; }

        [Display(Name = "Hướng dẫn sử dụng")]
        public string? UsageInstructions { get; set; }
    }

    /// <summary>Full form model bound to Prescriptions/Create (GET + POST).</summary>
    public class PrescriptionCreateViewModel
    {
        // Hidden — passed from MedicalRecords redirect
        public int MedicalRecordId { get; set; }

        // Display helpers (not posted)
        public string? PatientName { get; set; }
        public string? DoctorName { get; set; }
        public DateTime? RecordDate { get; set; }

        [Display(Name = "Lời dặn của bác sĩ")]
        public string? DoctorAdvice { get; set; }

        // The list of drug items — bound via index-based model binding
        public List<PrescriptionItemViewModel> Items { get; set; } = new()
        {
            new PrescriptionItemViewModel() // start with one empty row
        };
    }
}
