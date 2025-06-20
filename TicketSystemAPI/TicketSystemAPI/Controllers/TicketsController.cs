using System.Net.Sockets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Terminal;
using TicketSystemAPI.Data;
using TicketSystemAPI.DTO;
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
            if (user == null)
                return NotFound("User not found.");

            var paidTickets = await _context.Tickets
                .Include(t => t.Type)
                .Where(t => t.UserId == userId && t.Status == "Paid")
                .ToListAsync();

            var validTickets = new List<object>();
            var ticketsToUpdate = false;

            foreach (var ticket in paidTickets)
            {
                if (ticket.IsValid())
                {
                    validTickets.Add(new
                    {
                        TicketId = ticket.TicketId,
                        TicketType = ticket.Type.Name,
                        Expiration = ticket.ExpirationTime,
                        RideLimit = ticket.RideLimit,
                        RidesTaken = ticket.RidesTaken
                    });
                }
                else
                {
                    ticket.Status = "Expired";
                    ticketsToUpdate = true;
                }
            }

            if (ticketsToUpdate)
                await _context.SaveChangesAsync(); // Save expired statuses if any

            if (validTickets.Count == 0)
                return Ok(new { Valid = false, Message = "No valid tickets." });

            return Ok(new
            {
                Valid = true,
                Tickets = validTickets
            });
        }

        // Get entry history for a specific ticket
        [HttpGet("{ticketId}/entries")]
        public async Task<IActionResult> GetTicketEntryHistory(int ticketId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null)
                return NotFound("Ticket not found");

            var entries = await _context.TicketEntryHistories
                .Where(e => e.TicketId == ticketId)
                .OrderByDescending(e => e.ScannedAt)
                .Select(e => new
                {
                    e.ScannedAt
                })
                .ToListAsync();

            return Ok(entries);
        }

        // Get all entry history for a specific user
        [HttpGet("user/{userId}/entries")]
        public async Task<IActionResult> GetUserEntryHistory(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found");

            var entries = await _context.TicketEntryHistories
                .Include(e => e.Ticket)
                    .ThenInclude(t => t.Type)
                .Where(e => e.Ticket.UserId == userId)
                .OrderByDescending(e => e.ScannedAt)
                .Select(e => new
                {
                    TicketId = e.TicketId,
                    TicketType = e.Ticket.Type.Name,
                    RidesTaken = e.Ticket.RidesTaken,
                    e.ScannedAt
                })
                .ToListAsync();

            return Ok(entries);
        }



        // POST: api/tickets
        [HttpPost]
        public async Task<ActionResult<Ticket>> PostTicket(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTicketById), new { id = ticket.TicketId }, ticket);
        }

        // Price calculation (before checkout) - i.e. check the cart
        [HttpPost("calculate-price")]
        public async Task<ActionResult<PriceCalculationResponse>> CalculatePrice([FromBody] PriceCalculationRequest request)
        {
            var type = await _context.Tickettypes.FindAsync(request.TypeId);
            if (type == null || type.BasePrice == null)
                return BadRequest("Invalid ticket type");

            decimal basePrice = type.BasePrice.Value;
            decimal finalPrice = basePrice;
            decimal? discount = null;
            bool promoValid = false;

            if (!string.IsNullOrEmpty(request.DiscountCode))
            {
                var promo = await _context.Promotions.FirstOrDefaultAsync(p =>
                    p.PromoCode == request.DiscountCode &&
                    p.StartDate <= DateTime.UtcNow &&
                    p.EndDate >= DateTime.UtcNow);

                if (promo != null)
                {
                    discount = basePrice * (promo.DiscountPercentage / 100m);
                    finalPrice -= discount.Value;
                    promoValid = true;
                }
            }

            return Ok(new PriceCalculationResponse
            {
                BasePrice = basePrice,
                FinalPrice = finalPrice,
                DiscountApplied = discount,
                PromoValid = promoValid
            });
        }

        // Reserve and create payment intent (for checkout)
        [HttpPost("reserve-and-pay")]
        public async Task<IActionResult> ReserveAndCreatePaymentIntent([FromBody] TicketPurchaseRequest request)
        {

            var ticket = new Ticket
            {
                UserId = request.UserId,
                TypeId = request.TypeId,
                DiscountCode = request.DiscountCode,
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

            decimal finalPrice = ticketType.BasePrice.Value;

            // Check and Apply promotion if necessary
            if (!string.IsNullOrEmpty(request.DiscountCode))
            {
                var promo = await _context.Promotions.FirstOrDefaultAsync(p =>
                        p.PromoCode == request.DiscountCode &&
                        p.StartDate <= DateTime.UtcNow &&
                        p.EndDate >= DateTime.UtcNow);

                if (promo != null)
                {
                    finalPrice -= finalPrice * (promo.DiscountPercentage / 100m);
                }
            }

            var amountInGrosze = (long)(finalPrice * 100);  // Convert PLN price to grosze (Stripe amount is in the smallest currency unit)

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

            // Log to entry history
            var entry = new TicketEntryHistory
            {
                TicketId = ticketId,
                ScannedAt = DateTime.UtcNow
            };
            _context.TicketEntryHistories.Add(entry);

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
