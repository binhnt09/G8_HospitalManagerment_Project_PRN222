namespace G8_HospitalManagerment_Project_PRN222.ViewModels
{
    // ─────────────────────────────────────────────
    // Feature 1: View Medical History (GET)
    // ─────────────────────────────────────────────

    /// <summary>Individual prescribed drug inside a medical record.</summary>
    public class PrescribedDrugDto
    {
        public string DrugName { get; set; } = string.Empty;
        public string? Dosage { get; set; }
        public int? Quantity { get; set; }
        public int? DurationDays { get; set; }
        public string? UsageInstructions { get; set; }
    }

    /// <summary>A single prescription attached to a medical record.</summary>
    public class PrescriptionDto
    {
        public int PrescriptionId { get; set; }
        public DateTime? PrescriptionDate { get; set; }
        public string? DoctorAdvice { get; set; }
        public List<PrescribedDrugDto> Drugs { get; set; } = new();
    }

    /// <summary>One visit record returned in the patient's medical history.</summary>
    public class MedicalHistoryRecordDto
    {
        public int RecordId { get; set; }
        public DateTime? DateOfVisit { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string? Diagnosis { get; set; }
        public string? Symptoms { get; set; }
        public string? Treatment { get; set; }
        public List<PrescriptionDto> Prescriptions { get; set; } = new();
    }

    // ─────────────────────────────────────────────
    // Feature 2: Add Medical Record (POST Input)
    // ─────────────────────────────────────────────

    /// <summary>Request body for creating a new medical record after an appointment.</summary>
    public class AddMedicalRecordDto
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public string? Diagnosis { get; set; }
        public string? Symptoms { get; set; }
        public string? Treatment { get; set; }
    }

    // ─────────────────────────────────────────────
    // Feature 3: Add E-Prescription (POST Input)
    // ─────────────────────────────────────────────

    /// <summary>Individual drug line item in a prescription request.</summary>
    public class PrescriptionItemRequestDto
    {
        public int DrugId { get; set; }
        public int Quantity { get; set; }
        public string? Dosage { get; set; }
        public int? DurationDays { get; set; }
        public string? UsageInstructions { get; set; }
    }

    /// <summary>Request body for creating an electronic prescription with multiple drug items.</summary>
    public class AddPrescriptionDto
    {
        public int MedicalRecordId { get; set; }
        public string? DoctorAdvice { get; set; }
        public List<PrescriptionItemRequestDto> Items { get; set; } = new();
    }
}
