using System.ComponentModel.DataAnnotations;

namespace G8_HospitalManagerment_Project_PRN222.Models.Attributes
{
    public class MinAgeAttribute : ValidationAttribute
    {
        private readonly int _minAge;

        public MinAgeAttribute(int minAge)
        {
            _minAge = minAge;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime dateOfBirth)
            {
                // Tính toán tuổi dựa trên thời điểm hiện tại
                var today = DateTime.Today;
                var age = today.Year - dateOfBirth.Year;

                // Kiểm tra nếu chưa đến sinh nhật trong năm nay
                if (dateOfBirth.Date > today.AddYears(-age))
                    age--;

                if (age >= _minAge)
                {
                    return ValidationResult.Success;
                }

                return new ValidationResult(ErrorMessage ?? $"Bạn phải ít nhất {_minAge} tuổi.");
            }

            return new ValidationResult("Ngày sinh không hợp lệ.");
        }
    }
}