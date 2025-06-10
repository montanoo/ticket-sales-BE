using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSystemAPI.Data;
using TicketSystemAPI.DTO;
using TicketSystemAPI.Helpers;
using TicketSystemAPI.Models;

namespace TicketSystemAPI.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminUserController : ControllerBase
    {
        private readonly TicketSystemContext _context;

        public AdminUserController(TicketSystemContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            return await _context.Users
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    Name = u.Name,
                    Role = u.Role
                }).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Name = user.Name,
                Role = user.Role
            };
        }

        [HttpPut("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id, ResetPasswordDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var passwordHasher = new PasswordHasher<User>();
            user.Password = passwordHasher.HashPassword(user, dto.NewPassword);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/update-permissions")]
        public async Task<IActionResult> UpdatePermissions(int id, UpdatePermissionsDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Role = dto.Role;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
