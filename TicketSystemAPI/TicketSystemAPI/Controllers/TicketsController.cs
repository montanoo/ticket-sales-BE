using Microsoft.AspNetCore.Mvc;
using TicketSystemAPI.Models;
using TicketSystemAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Net.Sockets;

namespace TicketSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly TicketSystemContext _context;

        public TicketsController(TicketSystemContext context)
        {
            _context = context;
        }


        [HttpPost("reserve")]
        public async Task<IActionResult> ReserveTicket([FromBody] TicketPurchaseRequest request)
        {

            var ticket = new Ticket
            {
                UserId = request.UserId,
                TypeId = request.TypeId,
                Status = "Reserved",
                ReservedAt = DateTime.UtcNow,
                ExpirationTime = DateTime.UtcNow.AddMinutes(10) // Set ExpirationTime to 10 mins from now
            };

            await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();

            // Return TicketId to frontend for payment
            return Ok(new { ticketId = ticket.TicketId });
        }

        // GET: api/users
        // Register or Create account by the user through frontend
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] User newUser)
        {
            if (string.IsNullOrEmpty(newUser.Email) || string.IsNullOrEmpty(newUser.Password))
                return BadRequest("Email and Password are required.");

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == newUser.Email);
            if (existingUser != null)
                return Conflict("Email is already registered.");

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserById), new { id = newUser.UserId }, newUser);
        }

        [HttpGet("get-user/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return Ok(new { user.UserId, user.Email });
        }


        // For mobile part to get the user id
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                return Unauthorized("Invalid email or password");

            // Simple plain-text check (will be corrected with auth later)
            if (user.Password != request.Password)
                return Unauthorized("Invalid email or password");

            return Ok(new { user.UserId, user.Email });

            // Optionally generate a JWT token here for auth
            //return Ok(new { user.UserId, user.Email /*, token = ...*/ });
        }

        // Verify user by scanning QR code from mobile side
        [HttpGet("verify-mobile/{userId}")]
        public async Task<IActionResult> VerifyUser(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found.");

            var activeTicket = await _context.Tickets
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.ExpirationTime)
                .FirstOrDefaultAsync();

            if (activeTicket == null )
                return Ok(new { Valid = false, Message = "No valid ticket." });

            // Check ticket validity and update DB if it's no longer valid
            if (!activeTicket.IsValid())
            {
                activeTicket.Status = "Expired";
                await _context.SaveChangesAsync(); // Save the status change to database
                return Ok(new { Valid = false, Message = "No valid ticket." });
            }

            return Ok(new
            {
                Valid = true,
                TicketId = activeTicket.TicketId,
                Expiration = activeTicket.ExpirationTime,
                RideLimit = activeTicket.RideLimit,
                RidesTaken = activeTicket.RidesTaken
            });
        }




        // GET: api/tickets
        [HttpGet("getAllTickets")]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTickets()
        {
            var tickets = await _context.Tickets
            .Include(t => t.User)
            .Include(t => t.Type)
            .Include(t => t.Payment)
            .Select(t => new
             {
                t.TicketId,
                t.PurchaseTime,
                t.TypeId,
                t.ExpirationTime,
                t.RidesTaken,
                t.RideLimit,
                Price = t.Price.HasValue ? Math.Round(t.Price.Value, 2) : (decimal?)null,
                t.DiscountCode,
                t.UserId,
                UserEmail = t.User.Email,
                TicketTypeName = t.Type.Name,
                Payment = t.Payment != null ? new
                {
                    t.Payment.PaymentId,
                    Amount = Math.Round(t.Payment.Amount, 2),
                    t.Payment.Method
                } : null
                }).ToListAsync();

            return Ok(tickets);
        }

        [HttpGet("get-ticket/{id}")]
        public async Task<IActionResult> GetTicketById(int id)
        {
            var ticket = await _context.Tickets
            .Include(t => t.User)
            .Include(t => t.Type)
            .Include(t => t.Payment)
            .FirstOrDefaultAsync(t => t.TicketId == id);

            if (ticket == null) return NotFound();

            if (!ticket.IsValid())
                return BadRequest("Ticket is no longer valid.");

            var result = new
            {
                ticket.TicketId,
                ticket.PurchaseTime,
                ticket.TypeId,
                ticket.ExpirationTime,
                ticket.RidesTaken,
                ticket.RideLimit,
                Price = ticket.Price.HasValue ? Math.Round(ticket.Price.Value, 2) : (decimal?)null,
                ticket.DiscountCode,
                ticket.UserId,
                UserEmail = ticket.User.Email,
                TicketTypeName = ticket.Type.Name,
                Payment = ticket.Payment != null ? new
                {
                    ticket.Payment.PaymentId,
                    Amount = Math.Round(ticket.Payment.Amount, 2),
                    ticket.Payment.Method
                } : null
            };

            return Ok(result);
        }

        // GET: api/users/{userId}/tickets
        [HttpGet("/api/users/{userId}/tickets")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserTickets(int userId)
        {
            var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists)
            {
                return NotFound("User not found.");
            }

            var tickets = await _context.Tickets
                .Where(t => t.UserId == userId)
                .Include(t => t.Type)
                .Include(t => t.Payment)
                .Select(t => new
                {
                    t.TicketId,
                    t.PurchaseTime,
                    t.ExpirationTime,
                    t.RidesTaken,
                    t.RideLimit,
                    Price = t.Price.HasValue ? Math.Round(t.Price.Value, 2) : (decimal?)null,
                    t.DiscountCode,
                    TicketTypeName = t.Type.Name,
                    Payment = t.Payment != null ? new
                    {
                        t.Payment.PaymentId,
                        Amount = Math.Round(t.Payment.Amount, 2),
                        t.Payment.Method
                    } : null

                })
                .ToListAsync();

            return Ok(tickets);
        }



        // POST: api/tickets
        [HttpPost]
        public async Task<ActionResult<Ticket>> PostTicket(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTicketById), new { id = ticket.TicketId }, ticket);
        }


        [HttpPost("purchase")]
        public async Task<IActionResult> PurchaseTicket([FromBody] TicketPurchaseRequest request)
        {
            // First check if user exists
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            // Find ticket type
            var ticketType = await _context.Tickettypes.FirstOrDefaultAsync(t => t.TypeId == request.TypeId);
            if (ticketType == null)
            {
                return BadRequest("Ticket type not found.");
            }

            // Calculate price
            decimal basePrice = ticketType.BasePrice ?? 0m;
            //decimal basePrice = request.Variant switch
            //{
            //    "1-day" => 19.99m,
            //    "1-week" => 49.99m,
            //    "1-month" => 99.99m,
            //    "3-day-limited" => 29.99m,
            //    _ => 0m
            //};

            // Handle discount codes
            if (!string.IsNullOrEmpty(request.DiscountCode))
            {
                if (request.DiscountCode == "PROMO10")
                    basePrice *= 0.9m;
            }

            // Create a simple Payment
            var payment = new Payment
            {
                Amount = basePrice,
                Method = "Card" 
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync(); // Save so payment ID is generated

            Console.WriteLine($"Payment created with ID: {payment.PaymentId}");
            //Console.WriteLine(payment.PaymentId);

            // Create ticket
            var ticket = new Ticket
            {
                PurchaseTime = DateTime.Now,
                TypeId = ticketType.TypeId,
                ExpirationTime = DateTime.Now.AddDays(ticketType.BaseDurationDays ?? 30), // default 30 days if null
                RideLimit = (uint?)ticketType.BaseRideLimit,
                Price = Math.Round(basePrice, 2),
                DiscountCode = request.DiscountCode,
                UserId = request.UserId,
                PaymentId = payment.PaymentId // Ensure the ticket links to the payment
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTicketById), new { id = ticket.TicketId }, ticket);
        }


        // PUT: api/tickets/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTicket(int id, Ticket ticket)
        {
            if (id != ticket.TicketId)
            {
                return BadRequest();
            }

            _context.Entry(ticket).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{ticketId}/incrementRide")]
        public async Task<IActionResult> IncrementRide(int ticketId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);

            if (ticket == null)
                return NotFound("Ticket not found");

            // Check validity before increment
            if (!ticket.IsValid())
            {
                ticket.Status = "Expired";
                await _context.SaveChangesAsync(); // Save the status change to database
                return BadRequest("Ticket is not valid (expired, unpaid, or used up).");
            }

            ticket.RidesTaken = (ticket.RidesTaken ?? 0) + 1;

            await _context.SaveChangesAsync();

            return Ok(ticket);
        }

        // DELETE: api/tickets/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
