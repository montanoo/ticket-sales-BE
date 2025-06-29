using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TicketSystemAPI.Data;
using TicketSystemAPI.Models;
using TicketSystemAPI.DTO;
using Microsoft.EntityFrameworkCore;

namespace TicketSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly TicketSystemContext _context;

        public UserController(TicketSystemContext context)
        {
            _context = context;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
        {
            var userId = GetUserId();
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return NotFound();

            if (dto.Email != user.Email && _context.Users.Any(u => u.Email == dto.Email))
                return BadRequest("Email already in use.");

            user.Name = dto.Name;
            user.Email = dto.Email;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Profile updated successfully." });
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var userId = GetUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.Password, dto.OldPassword);

            if (result == PasswordVerificationResult.Failed)
                return BadRequest("Incorrect current password.");

            user.Password = hasher.HashPassword(user, dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password changed successfully." });
        }

        [HttpGet("purchase-history")]
        public async Task<IActionResult> PurchaseHistory()
        {
            var userId = GetUserId();

            var tickets = await _context.Tickets
                .Where(t => t.UserId == userId)
                .Include(t => t.Type) 
                .Include(t => t.Payment)
                .OrderByDescending(t => t.PurchaseTime)
                .Select(t => new
                {
                    t.TicketId,
                    t.PurchaseTime,
                    t.ExpirationTime,
                    TicketType = t.Type.Name,
                    t.Price,
                    PaymentStatus = t.Status
                })
                .ToListAsync();

            return Ok(tickets);
        }
    }
}
