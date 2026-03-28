using System.ComponentModel.DataAnnotations;

namespace G8_HospitalManagerment_Project_PRN222.Models
{
    public class PasswordReset
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string OtpCode { get; set; } = null!;
        public DateTime ExpiryTime { get; set; }
        public bool IsUsed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
