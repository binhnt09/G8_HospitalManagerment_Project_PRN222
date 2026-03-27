namespace G8_HospitalManagerment_Project_PRN222.ViewModels
{
    public class RegisterViewModel
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public string? FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string? Phone { get; set; }

        public string? Gender { get; set; }

        public DateTime? BirthDay { get; set; }

        public string? Address { get; set; }
    }
}
