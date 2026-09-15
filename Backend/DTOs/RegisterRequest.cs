using System.ComponentModel.DataAnnotations;

namespace Homebites.DTOs
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full Name must be between 2 and 100 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail\.com$", ErrorMessage = "Email must end with @gmail.com (e.g. user@gmail.com).")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile number is required.")]
        [RegularExpression(@"^\+91[6-9]\d{9}$", ErrorMessage = "Mobile must start with +91 followed by a valid 10-digit Indian number (e.g. +919876543210).")]
        public string Mobile { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required(ErrorMessage = "Identifier is required.")]
        public string Identifier { get; set; } = string.Empty; // Email ending with @gmail.com or Mobile starting with +91

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }

    public class OtpRequest
    {
        public string? Email { get; set; }
        public string? Mobile { get; set; }

        [Required(ErrorMessage = "OTP code is required.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must be exactly 6 digits.")]
        public string OtpCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Purpose is required.")]
        public string Purpose { get; set; } = "Registration";
    }

    public class AddressRequest
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Address Type is required.")]
        [RegularExpression(@"^(Home|Work|Other)$", ErrorMessage = "Address Type must be Home, Work, or Other.")]
        public string AddressType { get; set; } = "Home";

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile is required.")]
        [RegularExpression(@"^\+91[6-9]\d{9}$", ErrorMessage = "Mobile must start with +91 followed by 10 digits.")]
        public string Mobile { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address Line 1 is required.")]
        [StringLength(200, MinimumLength = 5)]
        public string AddressLine1 { get; set; } = string.Empty;

        public string? AddressLine2 { get; set; }
        public string? Landmark { get; set; }

        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; } = "Hyderabad";

        [Required(ErrorMessage = "State is required.")]
        public string State { get; set; } = "Telangana";

        [Required(ErrorMessage = "Pincode is required.")]
        [RegularExpression(@"^[1-9]\d{5}$", ErrorMessage = "Pincode must be exactly 6 digits (e.g. 500081).")]
        public string Pincode { get; set; } = string.Empty;

        public bool IsDefault { get; set; } = false;
    }
}
