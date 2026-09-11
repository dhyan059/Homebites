using System.ComponentModel.DataAnnotations;

namespace Homebites.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(15)]
        public string Mobile { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Role { get; set; } = "Customer"; // Customer | Kitchen | Admin

        public bool IsEmailVerified  { get; set; } = false;
        public bool IsMobileVerified { get; set; } = false;
        public bool IsActive         { get; set; } = true;

        public string? ProfileImageUrl  { get; set; }
        public DateTime? LastLoginAt    { get; set; }
        public DateTime? PasswordChangedAt { get; set; }
        public DateTime? DeactivatedAt  { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}