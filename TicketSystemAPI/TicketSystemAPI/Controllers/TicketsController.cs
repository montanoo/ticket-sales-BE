using System.Net.Sockets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using TicketSystemAPI.Data;
using TicketSystemAPI.Models;

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

        // GET: api/tickets
        [HttpGet("get-all-tickets")]
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

            if (activeTicket == null)
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



        // POST: api/tickets
        [HttpPost]
        public async Task<ActionResult<Ticket>> PostTicket(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTicketById), new { id = ticket.TicketId }, ticket);
        }

        // Reserve and create payment intent
        [HttpPost("reserve-and-pay")]
        public async Task<IActionResult> ReserveAndCreatePaymentIntent([FromBody] TicketPurchaseRequest request)
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

            var ticketType = await _context.Tickettypes.FindAsync(ticket.TypeId);

            if (ticketType == null || ticketType.BasePrice == null)
            {
                return BadRequest("Ticket type not found or has no base price.");
            }

            var amountInGrosze = (long)(ticketType.BasePrice.Value * 100); // Convert PLN price to grosze (Stripe amount is in the smallest currency unit)

            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInGrosze,
                Currency = "pln",
                PaymentMethodTypes = new List<string> { "card" },
                Metadata = new Dictionary<string, string>
                {
                    { "TicketId", ticket.TicketId.ToString() },
                    { "UserId", ticket.UserId.ToString() }
                }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            return Ok(new { clientSecret = paymentIntent.ClientSecret });  // payment intent created is returned with pi_...,
                                                                           // webhook endpoint will listen and wait for payment confirmation
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

        // Increment the ride taken of the ticket after scanning the QR code from mobile
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
