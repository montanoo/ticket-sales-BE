using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Tax;
using TicketSystemAPI.Data;
using TicketSystemAPI.DTO;
using TicketSystemAPI.Helpers;
using TicketSystemAPI.Models;

namespace TicketSystemAPI.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/config")]
    [Authorize(Roles = "Admin")]
    public class AdminConfigController : ControllerBase
    {
        private readonly TicketSystemContext _context;

        public AdminConfigController(TicketSystemContext context)
        {
            _context = context;
        }

        // Integration settings - not necessary as confirmed with supervisor

        // Promotions
        [HttpGet("promotions")]
        public async Task<ActionResult<IEnumerable<Promotion>>> GetPromotions()
        {
            var promotions = await _context.Promotions.ToListAsync();
            if (promotions == null || promotions.Count == 0)
                return NotFound("No promotions found.");
            return Ok(promotions);
        }

        [HttpPost("promotions")]
        public async Task<IActionResult> CreatePromotion([FromBody] PromotionCreateDto dto)
        {
            var promotion = new Promotion
            {
                PromoCode = dto.PromoCode,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                DiscountPercentage = dto.DiscountPercentage
            };

            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPromotions), new { id = promotion.Id }, promotion);
        }

        [HttpPut("promotions/{id}")]
        public async Task<IActionResult> UpdatePromotion(int id, [FromBody] PromotionUpdateDto dto)
        {
            var promo = await _context.Promotions.FindAsync(id);
            if (promo == null) return NotFound();

            promo.PromoCode = dto.PromoCode;
            promo.Description = dto.Description;
            promo.StartDate = dto.StartDate;
            promo.EndDate = dto.EndDate;
            promo.DiscountPercentage = dto.DiscountPercentage;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("promotions/{id}")]
        public async Task<IActionResult> DeletePromotion(int id)
        {
            var promo = await _context.Promotions.FindAsync(id);
            if (promo == null) return NotFound();

            _context.Promotions.Remove(promo);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Notifications config
        [HttpGet("notifications-config")]
        public async Task<ActionResult<NotificationConfig>> GetNotificationConfig()
        {
            var config = await _context.NotificationConfigs.FindAsync(1);
            if (config == null)
                return NotFound("Notification configuration not found.");
            return Ok(config);
        }

        [HttpPut("notifications-config")]
        public async Task<IActionResult> UpdateNotificationConfig([FromBody] NotificationConfig updated)
        {
            var config = await _context.NotificationConfigs.FindAsync(1);
            if (config == null)
            {
                updated.Id = 1; // enforce singleton
                _context.NotificationConfigs.Add(updated);
            }
            else
            {
                config.EmailSubjectTemplate = updated.EmailSubjectTemplate;
                config.EmailBodyTemplate = updated.EmailBodyTemplate;
                config.EnableEmailNotifications = updated.EnableEmailNotifications;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }











    }
}
