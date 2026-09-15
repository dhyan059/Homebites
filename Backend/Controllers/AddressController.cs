using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Homebites.Data;
using Homebites.Models;
using Homebites.DTOs;

namespace Homebites.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddressController : ControllerBase
    {
        private readonly HomebitesDbContext _context;

        public AddressController(HomebitesDbContext context)
        {
            _context = context;
        }

        // GET: api/address/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserAddresses(int userId)
        {
            var addresses = await _context.UserAddresses
                .Where(a => a.UserId == userId && !a.IsDeleted)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();

            return Ok(new { success = true, data = addresses });
        }

        // POST: api/address
        [HttpPost]
        public async Task<IActionResult> AddAddress([FromBody] AddressRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { success = false, message = "Validation failed.", errors });
            }

            if (!request.Mobile.StartsWith("+91") || request.Mobile.Length != 13)
            {
                return BadRequest(new { success = false, message = "Mobile must start with +91 and contain a 10-digit number." });
            }

            if (request.Pincode.Length != 6 || !request.Pincode.All(char.IsDigit))
            {
                return BadRequest(new { success = false, message = "Pincode must be exactly 6 digits." });
            }

            var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId);
            if (!userExists)
            {
                return NotFound(new { success = false, message = "User not found." });
            }

            if (request.IsDefault)
            {
                var existingDefaults = await _context.UserAddresses
                    .Where(a => a.UserId == request.UserId && a.IsDefault)
                    .ToListAsync();
                foreach (var d in existingDefaults) d.IsDefault = false;
            }

            var address = new UserAddress
            {
                UserId = request.UserId,
                AddressType = request.AddressType,
                FullName = request.FullName.Trim(),
                Mobile = request.Mobile.Trim(),
                AddressLine1 = request.AddressLine1.Trim(),
                AddressLine2 = request.AddressLine2?.Trim(),
                Landmark = request.Landmark?.Trim(),
                City = request.City.Trim(),
                State = request.State.Trim(),
                Pincode = request.Pincode.Trim(),
                IsDefault = request.IsDefault,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserAddresses.Add(address);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Address saved successfully.", data = address });
        }

        // PUT: api/address/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAddress(int id, [FromBody] AddressRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { success = false, message = "Validation failed.", errors });
            }

            var address = await _context.UserAddresses.FindAsync(id);
            if (address == null)
            {
                return NotFound(new { success = false, message = "Address not found." });
            }

            if (address.UserId != request.UserId)
            {
                return Unauthorized(new { success = false, message = "Unauthorized to update this address." });
            }

            if (!request.Mobile.StartsWith("+91") || request.Mobile.Length != 13)
            {
                return BadRequest(new { success = false, message = "Mobile must start with +91 and contain a 10-digit number." });
            }

            if (request.Pincode.Length != 6 || !request.Pincode.All(char.IsDigit))
            {
                return BadRequest(new { success = false, message = "Pincode must be exactly 6 digits." });
            }

            if (request.IsDefault)
            {
                var existingDefaults = await _context.UserAddresses
                    .Where(a => a.UserId == request.UserId && a.Id != id && a.IsDefault)
                    .ToListAsync();
                foreach (var d in existingDefaults) d.IsDefault = false;
            }

            address.AddressType = request.AddressType;
            address.FullName = request.FullName.Trim();
            address.Mobile = request.Mobile.Trim();
            address.AddressLine1 = request.AddressLine1.Trim();
            address.AddressLine2 = request.AddressLine2?.Trim();
            address.Landmark = request.Landmark?.Trim();
            address.City = request.City.Trim();
            address.State = request.State.Trim();
            address.Pincode = request.Pincode.Trim();
            address.IsDefault = request.IsDefault;
            address.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Address updated successfully.", data = address });
        }

        // DELETE: api/address/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(int id, [FromQuery] int userId)
        {
            var address = await _context.UserAddresses.FindAsync(id);
            if (address == null)
            {
                return NotFound(new { success = false, message = "Address not found." });
            }

            if (address.UserId != userId)
            {
                return Unauthorized(new { success = false, message = "Unauthorized to delete this address." });
            }

            address.IsDeleted = true;
            address.IsDefault = false;
            address.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Address deleted successfully." });
        }
    }
}
