using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TicketSystemAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TicketSystemAPI.Data;
using TicketSystemAPI.DTOs;
using TicketSystemAPI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;


namespace TicketSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly TicketSystemContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(TicketSystemContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterDto dto)
        {
            if (_context.Users.Any(u => u.Email == dto.Email))
                return BadRequest("Email already registered.");

            var user = new User
            {
                Email = dto.Email,
                Name = dto.Name
            };

            var passwordHasher = new PasswordHasher<User>();
            user.Password = passwordHasher.HashPassword(user, dto.Password);
            _context.Users.Add(user);
            _context.SaveChanges();

            var jwt = JwtTokenGenerator.GenerateToken(user, _configuration["Jwt:Key"]);
            Response.Cookies.Append("auth", jwt, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax, 
                Secure = true,          
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });
            return Ok(new { token = jwt, userId = user.UserId, user });
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            Console.WriteLine("==== CALLBACK HIT ====");
            foreach (var c in Request.Cookies)
                Console.WriteLine($"{c.Key} = {c.Value}");

            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleCallback")
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

       [HttpGet("google/callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            Console.WriteLine("==== CALLBACK HIT ====");
            foreach (var cookie in Request.Cookies)
            {
                Console.WriteLine($"{cookie.Key} = {cookie.Value}");
            }

            // attempt to authenticate with the external scheme
            var result = await HttpContext.AuthenticateAsync();

            if (!result.Succeeded)
            {
                Console.WriteLine("Google auth failed.");
                return Unauthorized("Google auth failed.");
            }

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;
            Console.WriteLine($"Email: {email}, Name: {name}");

            if (email == null)
                return Unauthorized();

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                user = new User { Email = email, Name = name };
                _context.Users.Add(user);
                _context.SaveChanges();
            }

            var jwt = JwtTokenGenerator.GenerateToken(user, _configuration["Jwt:Key"]);
            Response.Cookies.Append("auth", jwt, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = false,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Redirect("http://localhost:5173");
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDto dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);
            if (user == null)
                return Unauthorized("Invalid credentials.");

            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher.VerifyHashedPassword(user, user.Password, dto.Password);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid credentials.");

            var jwt = JwtTokenGenerator.GenerateToken(user, _configuration["Jwt:Key"]);
            Response.Cookies.Append("auth", jwt, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });
            return Ok(new { token = jwt, userId = user.UserId, user });
        }
        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (userId == null || email == null)
                return Unauthorized();

            var user = _context.Users
            .Where(u => u.Email == email)
            .Select(u => new
            {
                u.UserId,
                u.Email,
                u.Name,
                u.Password
            })
            .FirstOrDefault();

            if (user == null)
                return Unauthorized();
            return Ok(new
            {
                user,
            });
        }
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("auth", new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = false // or true if using HTTPS
            });

            return Ok(new { message = "Logged out successfully" });
        }
    }
}
