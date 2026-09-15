using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Homebites.Data;
using Homebites.Models;
using Homebites.DTOs;
using System.Security.Cryptography;
using System.Text;

namespace Homebites.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly HomebitesDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AuthController(HomebitesDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { success = false, message = "Validation failed.", errors });
            }

            // Normalise
            string cleanEmail = request.Email.Trim().ToLowerInvariant();
            string cleanMobile = request.Mobile.Trim();

            if (IsAdminEmail(cleanEmail))
            {
                return BadRequest(new { success = false, message = "This email is reserved for the admin portal and cannot be registered as a customer." });
            }

            // Strict check: must end with @gmail.com
            if (!cleanEmail.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { success = false, message = "Email must end with @gmail.com." });
            }

            // Strict check: must start with +91
            if (!cleanMobile.StartsWith("+91") || cleanMobile.Length != 13)
            {
                return BadRequest(new { success = false, message = "Mobile must start with +91 and contain a 10-digit number (e.g. +919876543210)." });
            }

            // Check if user already exists
            bool emailExists = await _context.Users.AnyAsync(u => u.Email == cleanEmail);
            if (emailExists)
            {
                return Conflict(new { success = false, message = "A user with this Gmail address already exists." });
            }

            bool mobileExists = await _context.Users.AnyAsync(u => u.Mobile == cleanMobile);
            if (mobileExists)
            {
                return Conflict(new { success = false, message = "A user with this mobile number already exists." });
            }

            // Hash password with SHA256 (or salt/hash)
            string passwordHash = HashPassword(request.Password);

            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = cleanEmail,
                Mobile = cleanMobile,
                PasswordHash = passwordHash,
                Role = "Customer",
                IsEmailVerified = false,
                IsMobileVerified = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            string testOtp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var otpRecord = new OtpVerification
            {
                UserId = user.Id,
                Email = cleanEmail,
                Mobile = cleanMobile,
                EmailOtp = testOtp,
                MobileOtp = testOtp,
                Purpose = "Registration",
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                CreatedAt = DateTime.UtcNow,
                IsUsed = false
            };

            _context.OtpVerifications.Add(otpRecord);
            await _context.SaveChangesAsync();

            var response = new Dictionary<string, object?>
            {
                ["success"] = true,
                ["message"] = "Registration successful! Verification OTP has been sent.",
                ["otp"] = testOtp,
                ["userId"] = user.Id,
                ["user"] = new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.Mobile,
                    user.Role
                }
            };

            return Ok(response);
        }

        // POST: api/auth/verify-otp
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] OtpRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid OTP request." });
            }

            var query = _context.OtpVerifications.AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                string email = request.Email.Trim().ToLowerInvariant();
                query = query.Where(o => o.Email == email);
            }
            else if (!string.IsNullOrWhiteSpace(request.Mobile))
            {
                string mobile = request.Mobile.Trim();
                query = query.Where(o => o.Mobile == mobile);
            }
            else
            {
                return BadRequest(new { success = false, message = "Email or Mobile is required for OTP verification." });
            }

            var otpEntry = await query
                .Where(o => o.Purpose == request.Purpose && !o.IsUsed)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpEntry == null)
            {
                return BadRequest(new { success = false, message = "No valid OTP request found." });
            }

            if (DateTime.UtcNow > otpEntry.ExpiresAt)
            {
                return BadRequest(new { success = false, message = "OTP has expired. Please request a new one." });
            }

            if (otpEntry.EmailOtp != request.OtpCode && otpEntry.MobileOtp != request.OtpCode)
            {
                return BadRequest(new { success = false, message = "Invalid 6-digit OTP code." });
            }

            otpEntry.IsUsed = true;
            otpEntry.VerifiedAt = DateTime.UtcNow;
            otpEntry.EmailVerified = true;
            otpEntry.MobileVerified = true;

            if (otpEntry.UserId.HasValue)
            {
                var user = await _context.Users.FindAsync(otpEntry.UserId.Value);
                if (user != null)
                {
                    user.IsEmailVerified = true;
                    user.IsMobileVerified = true;
                    user.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "OTP verified successfully!" });
        }

        // POST: api/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] PasswordResetRequest request)
        {
            string identifier = request.Identifier?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return BadRequest(new { success = false, message = "Email or mobile number is required." });
            }

            string normalizedEmail = identifier.ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Email == normalizedEmail || u.Mobile == identifier);

            if (user == null)
            {
                return NotFound(new { success = false, message = "No account was found with that email or mobile number." });
            }

            string resetOtp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            _context.OtpVerifications.Add(new OtpVerification
            {
                UserId = user.Id,
                Email = user.Email,
                Mobile = user.Mobile,
                EmailOtp = resetOtp,
                MobileOtp = resetOtp,
                Purpose = "ForgotPassword",
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                CreatedAt = DateTime.UtcNow,
                IsUsed = false
            });
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"[Password Reset Error] {exception}");
                return StatusCode(500, new { success = false, message = "The password reset request could not be saved. Please restart the backend and try again." });
            }

            var response = new Dictionary<string, object?>
            {
                ["success"] = true,
                ["message"] = "A password reset OTP has been sent.",
                ["otp"] = resetOtp,
                ["identifier"] = user.Email
            };

            return Ok(response);
        }

        // POST: api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] PasswordResetRequest request)
        {
            string identifier = request.Identifier?.Trim() ?? string.Empty;
            string normalizedEmail = identifier.ToLowerInvariant();
            string otpCode = request.OtpCode?.Trim() ?? string.Empty;
            string newPassword = request.NewPassword ?? string.Empty;

            if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(otpCode) || newPassword.Length < 8)
            {
                return BadRequest(new { success = false, message = "Email/mobile, OTP, and a password of at least 8 characters are required." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Email == normalizedEmail || u.Mobile == identifier);
            if (user == null)
            {
                return BadRequest(new { success = false, message = "Account not found." });
            }

            var otpEntry = await _context.OtpVerifications
                .Where(o => o.UserId == user.Id && o.Purpose == "ForgotPassword" && !o.IsUsed)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpEntry == null || DateTime.UtcNow > otpEntry.ExpiresAt)
            {
                return BadRequest(new { success = false, message = "The reset OTP is invalid or expired. Please request a new one." });
            }

            if (otpEntry.EmailOtp != otpCode && otpEntry.MobileOtp != otpCode)
            {
                return BadRequest(new { success = false, message = "Invalid reset OTP." });
            }

            user.PasswordHash = HashPassword(newPassword);
            user.PasswordChangedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            otpEntry.IsUsed = true;
            otpEntry.VerifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Password updated successfully. You can now log in." });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Identifier and password are required." });
            }

            string identifier = request.Identifier.Trim();
            string passwordHash = HashPassword(request.Password);

            if (IsAdminEmail(identifier))
            {
                return Unauthorized(new { success = false, message = "Admin accounts must sign in through the admin portal." });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => (u.Email == identifier.ToLower() || u.Mobile == identifier) && u.PasswordHash == passwordHash);

            if (user == null)
            {
                return Unauthorized(new { success = false, message = "Invalid credentials. Please check your email/mobile or password." });
            }

            if (!string.Equals(user.Role, "Customer", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new { success = false, message = "This account must sign in through the appropriate portal." });
            }

            if (!user.IsActive)
            {
                return StatusCode(403, new { success = false, message = "Account is inactive. Please contact support." });
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Login successful!",
                user = new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.Mobile,
                    user.Role
                }
            });
        }

        private static bool IsAdminEmail(string email)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            return normalizedEmail == "adminbites@gmail.com" || normalizedEmail == "admin@bites.in";
        }


        // PUT: api/auth/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null) return NotFound(new { success = false, message = "User not found." });

            if (!string.IsNullOrWhiteSpace(request.FullName))
                user.FullName = request.FullName.Trim();

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Profile updated.",
                user = new { user.Id, user.FullName, user.Email, user.Mobile, user.Role } });
        }

        // POST: api/auth/update-mobile
        [HttpPost("update-mobile")]
        public async Task<IActionResult> UpdateMobile([FromBody] UpdateContactRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null) return NotFound(new { success = false, message = "User not found." });

            string cleanMobile = request.NewValue?.Trim() ?? "";
            if (!System.Text.RegularExpressions.Regex.IsMatch(cleanMobile, @"^\+91[6-9]\d{9}$"))
                return BadRequest(new { success = false, message = "Mobile must start with +91 followed by 10 valid digits." });

            bool exists = await _context.Users.AnyAsync(u => u.Mobile == cleanMobile && u.Id != request.UserId);
            if (exists) return Conflict(new { success = false, message = "This mobile is already registered." });

            string otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            _context.OtpVerifications.Add(new OtpVerification {
                UserId = user.Id, Email = user.Email, Mobile = cleanMobile,
                EmailOtp = otp, MobileOtp = otp, Purpose = "ChangeMobile",
                ExpiresAt = DateTime.UtcNow.AddMinutes(10), CreatedAt = DateTime.UtcNow, IsUsed = false
            });
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"[Mobile OTP Error] {exception}");
                return StatusCode(500, new { success = false, message = "Unable to save the mobile OTP. Check the backend console for details." });
            }
            return Ok(new { success = true, message = "OTP generated successfully.", otp });
        }

        // POST: api/auth/confirm-mobile
        [HttpPost("confirm-mobile")]
        public async Task<IActionResult> ConfirmMobileUpdate([FromBody] UpdateContactConfirmRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null) return NotFound(new { success = false });
            var otp = await _context.OtpVerifications
                .Where(o => o.UserId == request.UserId && o.Purpose == "ChangeMobile" && !o.IsUsed)
                .OrderByDescending(o => o.CreatedAt).FirstOrDefaultAsync();
            if (otp == null || DateTime.UtcNow > otp.ExpiresAt)
                return BadRequest(new { success = false, message = "OTP expired or invalid." });
            if (!System.Text.RegularExpressions.Regex.IsMatch(request.OtpCode?.Trim() ?? "", @"^\d{6}$") || otp.MobileOtp != request.OtpCode?.Trim())
                return BadRequest(new { success = false, message = "Wrong OTP." });
            otp.IsUsed = true;
            user.Mobile = request.NewValue?.Trim() ?? user.Mobile;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Mobile updated.",
                user = new { user.Id, user.FullName, user.Email, user.Mobile, user.Role } });
        }

        // POST: api/auth/update-email
        [HttpPost("update-email")]
        public async Task<IActionResult> UpdateEmail([FromBody] UpdateContactRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null) return NotFound(new { success = false });
            string cleanEmail = request.NewValue?.Trim().ToLowerInvariant() ?? "";
            if (!System.Text.RegularExpressions.Regex.IsMatch(cleanEmail, @"^[a-zA-Z0-9._%+-]+@gmail\.com$"))
                return BadRequest(new { success = false, message = "Must be a @gmail.com address." });
            bool exists = await _context.Users.AnyAsync(u => u.Email == cleanEmail && u.Id != request.UserId);
            if (exists) return Conflict(new { success = false, message = "Email already registered." });
            string otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            _context.OtpVerifications.Add(new OtpVerification {
                UserId = user.Id, Email = cleanEmail, Mobile = user.Mobile,
                EmailOtp = otp, MobileOtp = otp, Purpose = "ChangeEmail",
                ExpiresAt = DateTime.UtcNow.AddMinutes(10), CreatedAt = DateTime.UtcNow, IsUsed = false
            });
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"[Email OTP Error] {exception}");
                return StatusCode(500, new { success = false, message = "Unable to save the email OTP. Check the backend console for details." });
            }
            return Ok(new { success = true, message = "OTP generated successfully.", otp });
        }

        // POST: api/auth/confirm-email
        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmailUpdate([FromBody] UpdateContactConfirmRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null) return NotFound(new { success = false });
            var otp = await _context.OtpVerifications
                .Where(o => o.UserId == request.UserId && o.Purpose == "ChangeEmail" && !o.IsUsed)
                .OrderByDescending(o => o.CreatedAt).FirstOrDefaultAsync();
            if (otp == null || DateTime.UtcNow > otp.ExpiresAt)
                return BadRequest(new { success = false, message = "OTP expired." });
            if (!System.Text.RegularExpressions.Regex.IsMatch(request.OtpCode?.Trim() ?? "", @"^\d{6}$") || otp.EmailOtp != request.OtpCode?.Trim())
                return BadRequest(new { success = false, message = "Wrong OTP." });
            otp.IsUsed = true;
            user.Email = request.NewValue?.Trim().ToLowerInvariant() ?? user.Email;
            user.IsEmailVerified = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Email updated.",
                user = new { user.Id, user.FullName, user.Email, user.Mobile, user.Role } });
        }

        // POST: api/auth/change-password
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null) return NotFound(new { success = false });
            if (user.PasswordHash != HashPassword(request.CurrentPassword ?? ""))
                return BadRequest(new { success = false, message = "Current password is incorrect." });
            if ((request.NewPassword ?? "").Length < 6)
                return BadRequest(new { success = false, message = "Password must be at least 6 characters." });
            user.PasswordHash = HashPassword(request.NewPassword!);
            user.PasswordChangedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Password changed successfully." });
        }
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "_homebites_salt"));
            return Convert.ToBase64String(bytes);
        }
    }
}
