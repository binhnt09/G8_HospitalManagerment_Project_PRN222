

using G8_HospitalManagerment_Project_PRN222.Models.Attributes;
using System.ComponentModel.DataAnnotations;

    namespace G8_HospitalManagerment_Project_PRN222.Models.ViewModels
    {
        public class RegisterViewModel
        {
            [Required(ErrorMessage = "Email không được để trống")]
            [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ")]
            public string Email { get; set; } = null!;

            [Required(ErrorMessage = "Mật khẩu không được để trống")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = null!;

            [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
            public string ConfirmPassword { get; set; } = null!;

            public string? FirstName { get; set; }

            [Required(ErrorMessage = "Tên không được để trống")]
            public string LastName { get; set; } = null!;

            [Required(ErrorMessage = "Số điện thoại không được để trống")]
            [RegularExpression(@"^(0[3|5|7|8|9])([0-9]{8})$", ErrorMessage = "Số điện thoại Việt Nam không hợp lệ")]
            public string? Phone { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn giới tính")]
            public string? Gender { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn ngày sinh")]
            [DataType(DataType.Date)]
            [MinAge(1, ErrorMessage = "Người dùng phải từ 16 tuổi trở lên để đăng ký.")]    
            public DateTime? BirthDay { get; set; }

            [StringLength(255, ErrorMessage = "Địa chỉ quá dài")]
            public string? Address { get; set; }
        }
    }

