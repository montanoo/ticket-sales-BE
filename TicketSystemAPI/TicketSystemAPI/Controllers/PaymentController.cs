using Microsoft.AspNetCore.Mvc;
using Stripe;
using TicketSystemAPI.Models;
using TicketSystemAPI.Data;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace TicketSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly string webhookSecret = "whsec_...";    // Set your webhook secret here for stripe cli listening
        private readonly TicketSystemContext _context;
        private readonly IEmailService _emailService;

        public PaymentController(TicketSystemContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // Payment Intent is created in TicketsController, where this webhook will listen for payment confirmation
        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    webhookSecret
                );
            }
            catch (StripeException e)
            {
                Console.WriteLine($"Stripe exception: {e.Message}");
                return BadRequest();
            }

            // Payment success handling
            if (stripeEvent.Type == "payment_intent.succeeded")
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;

                // Check for duplicate payments
                if (await _context.Payments.AnyAsync(p => p.StripePaymentIntentId == paymentIntent.Id))
                {
                    return Ok();
                }

                // Use TicketId directly from metadata
                if (!paymentIntent.Metadata.TryGetValue("TicketId", out var ticketIdStr) ||
                    !int.TryParse(ticketIdStr, out var ticketId))
                {
                    Console.WriteLine("Missing or invalid TicketId in metadata.");
                    return BadRequest();
                }

                var ticket = await _context.Tickets
                            .Include(t => t.Type)  // Include ticket type for duration
                            .FirstOrDefaultAsync(t => t.TicketId == ticketId);

                if (ticket == null)
                {
                    Console.WriteLine($"Ticket ID {ticketId} not found.");
                    return NotFound();
                }

                // Create payment record
                var payment = new Payment
                {
                    Amount = paymentIntent.AmountReceived / 100m, // cents to PLN
                    Method = "Stripe",
                    StripePaymentIntentId = paymentIntent.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Payments.AddAsync(payment);
                await _context.SaveChangesAsync();

                // Update ticket
                ticket.PaymentId = payment.PaymentId;
                ticket.PurchaseTime = DateTime.UtcNow;
                ticket.Status = "Paid";

                // Set ExpirationTime based on TicketType BaseDurationDays
                if (ticket.Type?.BaseDurationDays.HasValue == true)
                {
                    ticket.ExpirationTime = DateTime.UtcNow.AddDays(ticket.Type.BaseDurationDays.Value);
                }
                else
                {
                    ticket.ExpirationTime = null;  // Fallback if BaseDurationDays is not set
                }
                await _context.SaveChangesAsync();

                // Retrieve the user from metadata
                if (!paymentIntent.Metadata.TryGetValue("UserId", out var userIdStr) ||
                    !int.TryParse(userIdStr, out var userId))
                {
                    Console.WriteLine("Missing or invalid UserId in metadata.");
                    return BadRequest();
                }
                var user = await _context.Users.FirstOrDefaultAsync(t => t.UserId == userId);

                // Send email to notify the user
                if (user != null)
                {
                    await _emailService.SendEmailAsync(
                        user.Email,
                        "Payment Confirmation - Ticket System",
                        $"Hello,<br><br>Your payment of ${(payment.Amount):0.00} was successful.<br>Your ticket is now confirmed.<br><br>Thank you."
                    );
                }

            }

            return Ok();
        }

        [HttpGet("payment-records")]
        public async Task<IActionResult> GetPaymentRecords([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var query = _context.Payments.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(p => p.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(p => p.CreatedAt <= endDate.Value);

            var payments = await query
                .Select(p => new
                {
                    p.PaymentId,
                    p.Amount,
                    p.Method,
                    p.CreatedAt,
                    p.StripePaymentIntentId,

                    // Get all tickets linked to this payment
                    Tickets = _context.Tickets
                        .Where(t => t.PaymentId == p.PaymentId)
                        .Select(t => new
                        {
                            t.TicketId,
                            t.TypeId,
                            t.UserId,

                            // Include user email too
                            UserEmail = t.User.Email
                        }).ToList()
                })
                .ToListAsync();

            return Ok(payments);
        }

        [HttpGet("payment-records-specific-user")]
       // [Authorize(Roles = "Admin")] // Optional: if using auth
        public async Task<IActionResult> GetPaymentRecordsByUser([FromQuery] int? userId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var query = _context.Payments
                .Include(p => p.Tickets)
                .ThenInclude(t => t.User)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(p => p.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(p => p.CreatedAt <= toDate.Value);

            var result = await query.Select(p => new
            {
                p.PaymentId,
                p.Amount,
                p.Method,
                p.StripePaymentIntentId,
                p.CreatedAt,
                TicketId = p.Tickets.FirstOrDefault().TicketId,
                User = p.Tickets.FirstOrDefault().User.Email,
            })
            .Where(p => !userId.HasValue || _context.Tickets.Any(t => t.PaymentId == p.PaymentId && t.UserId == userId.Value))
            .ToListAsync();

            return Ok(result);
        }

        [HttpGet("test-email")]
        public async Task<IActionResult> SendTestEmail()
        {
            try
            {
                await _emailService.SendEmailAsync(
                    "test@localhost",
                    "Test Email",
                    "<h3>This is a test email sent from ASP.NET Core backend</h3>"
                );

                return Ok("Email sent successfully (check smtp4dev).");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Email send failed: {ex.Message}");
            }
        }
    }
}
