using System.ComponentModel.DataAnnotations;

namespace Homebites.Models
{
    public class OtpVerification
    {
        public long Id { get; set; }
        public int? UserId { get; set; }

        public string? Email  { get; set; }
        public string? Mobile { get; set; }

        public string? EmailOtp  { get; set; }
        public string? MobileOtp { get; set; }

        [Required, MaxLength(30)]
        public string Purpose { get; set; } = string.Empty;

        public bool EmailVerified  { get; set; } = false;
        public bool MobileVerified { get; set; } = false;
        public bool IsUsed         { get; set; } = false;

        public DateTime  ExpiresAt  { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime  CreatedAt  { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
    }
}