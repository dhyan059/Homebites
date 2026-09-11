using System.ComponentModel.DataAnnotations;

namespace Homebites.DTOs
{
    public class PasswordResetRequest
    {
        [Required]
        public string Identifier { get; set; } = string.Empty;

        public string OtpCode { get; set; } = string.Empty;

        public string NewPassword { get; set; } = string.Empty;
    }
}