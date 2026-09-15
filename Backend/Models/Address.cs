using System.ComponentModel.DataAnnotations;

namespace Homebites.Models
{
    public class UserAddress
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [Required, MaxLength(20)]
        public string AddressType { get; set; } = "Home";

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(15)]
        public string Mobile { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string AddressLine1 { get; set; } = string.Empty;

        public string? AddressLine2 { get; set; }
        public string? Landmark { get; set; }

        [Required, MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string State { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string Pincode { get; set; } = string.Empty;

        public bool IsDefault { get; set; } = false;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public User? User { get; set; }
    }
}