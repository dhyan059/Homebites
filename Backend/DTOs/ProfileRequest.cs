namespace Homebites.DTOs
{
    public class ProfileUpdateRequest
    {
        public int UserId { get; set; }
        public string? FullName { get; set; }
    }

    public class UpdateContactRequest
    {
        public int UserId { get; set; }
        public string? NewValue { get; set; }
    }

    public class UpdateContactConfirmRequest
    {
        public int UserId { get; set; }
        public string? NewValue { get; set; }
        public string? OtpCode { get; set; }
    }

    public class ChangePasswordRequest
    {
        public int UserId { get; set; }
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}
