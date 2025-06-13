using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSystemAPI.Data;
using TicketSystemAPI.DTO;
using TicketSystemAPI.Helpers;
using TicketSystemAPI.Models;

namespace TicketSystemAPI.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/sales")]
    [Authorize(Roles = "Admin")]
    public class AdminSalesController : ControllerBase
    {
        private readonly TicketSystemContext _context;

        public AdminSalesController(TicketSystemContext context)
        {
            _context = context;
        }

        // GET: /api/admin/sales/statistics
        [HttpGet("statistics")]
        public async Task<IActionResult> GetSalesStatistics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var payments = _context.Payments.Include(p => p.Tickets).AsQueryable();

            if (startDate.HasValue)
                payments = payments.Where(p => p.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                payments = payments.Where(p => p.CreatedAt <= endDate.Value);

            var paymentList = await payments.ToListAsync();

            var totalRevenue = paymentList.Sum(p => p.Amount);
            var totalTicketsSold = paymentList.SelectMany(p => p.Tickets).Count();
            var averagePrice = totalTicketsSold > 0 ? totalRevenue / totalTicketsSold : 0;

            var paymentMethodBreakdown = paymentList
                .GroupBy(p => p.Method)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

            return Ok(new
            {
                totalRevenue,
                totalTicketsSold,
                averageTicketPrice = Math.Round(averagePrice, 2),
                paymentMethodBreakdown
            });
        }

        // GET: /api/admin/sales/popularity
        [HttpGet("popularity")]
        public async Task<IActionResult> GetTicketTypePopularity([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var tickets = _context.Tickets
                .Include(t => t.Type)
                .Where(t => t.Status == "Paid");

            if (startDate.HasValue)
                tickets = tickets.Where(t => t.PurchaseTime >= startDate.Value);

            if (endDate.HasValue)
                tickets = tickets.Where(t => t.PurchaseTime <= endDate.Value);

            var data = await tickets
                .GroupBy(t => t.Type.Name)
                .Select(g => new
                {
                    ticketType = g.Key,
                    ticketsSold = g.Count(),
                    totalRevenue = g.Sum(t => t.Price ?? 0)
                })
                .ToListAsync();

            return Ok(data);
        }


    }
}
