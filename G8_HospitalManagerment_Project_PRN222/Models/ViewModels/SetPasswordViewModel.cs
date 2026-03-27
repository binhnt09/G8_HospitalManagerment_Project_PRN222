using System.ComponentModel.DataAnnotations;

namespace G8_HospitalManagerment_Project_PRN222.Models.ViewModels
{
    public class SetPasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
