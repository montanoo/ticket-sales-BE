using Microsoft.AspNetCore.Mvc;
using TicketSystemAPI.Models; // Assuming you have a Ticket model
using TicketSystemAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace TicketSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TicketsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/tickets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTickets()
        {
            var tickets = await _context.Tickets.Select(t => new
            {
                t.Id,
                t.Variant,
                Price = Math.Round(t.Price, 2), 
                t.MaxRides,
                t.DiscountCode,
                t.PurchaseDate
            }).ToListAsync();

            return Ok(tickets);
            //return await _context.Tickets.ToListAsync();
        }

        // GET: api/tickets/{id}
        //[HttpGet("{id}")]
        //public async Task<ActionResult<Ticket>> GetTicket(int id)
        //{
        //    var ticket = await _context.Tickets.FindAsync(id);

        //    if (ticket == null)
        //    {
        //        return NotFound();
        //    }

        //    return ticket;
        //}

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketById(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            var result = new
            {
                ticket.Id,
                ticket.Variant,
                Price = ticket.Price.ToString("F2"), // 👈 formats to 2 decimal places
                ticket.MaxRides,
                ticket.DiscountCode,
                ticket.PurchaseDate
            };

            return Ok(result);
            //return Ok(ticket);
        }

        // POST: api/tickets
        [HttpPost]
        public async Task<ActionResult<Ticket>> PostTicket(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTicket", new { id = ticket.Id }, ticket);
        }

        [HttpPost("purchase")]
        public async Task<IActionResult> PurchaseTicket([FromBody] TicketPurchaseRequest request)
        {
            decimal basePrice = request.Variant switch
            {
                "1-day" => 19.99m,
                "1-week" => 49.99m,
                "1-month" => 99.99m,
                "3-day-limited" => 29.99m,
                _ => 0m
            };

            if (basePrice == 0)
                return BadRequest("Invalid variant.");

            // Handle discount codes
            if (!string.IsNullOrEmpty(request.DiscountCode))
            {
                if (request.DiscountCode == "PROMO10")
                    basePrice *= 0.9m;
            }

            var ticket = new Ticket
            {
                Variant = request.Variant,
                Price = basePrice,
                MaxRides = request.Variant.Contains("limited") ? 10 : null,
                DiscountCode = request.DiscountCode,
                PurchaseDate = DateTime.Now
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTicketById), new { id = ticket.Id }, ticket);
        }


        // PUT: api/tickets/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTicket(int id, Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return BadRequest();
            }

            _context.Entry(ticket).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
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
