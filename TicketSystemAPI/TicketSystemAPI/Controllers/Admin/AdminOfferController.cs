using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TicketSystemAPI.Data;
using TicketSystemAPI.Models;
using TicketSystemAPI.DTO;
using TicketSystemAPI.Helpers;

namespace TicketSystemAPI.Controllers.Admin

{
    [ApiController]
    [Route("api/admin/offers")]
    [Authorize]      
    //[Authorize(Roles = "Admin")]

    public class AdminOfferController : ControllerBase
    {
        private readonly TicketSystemContext _context;
        private readonly IPassService _passService;

        public AdminOfferController(TicketSystemContext context, IPassService passService)
        {
            _context = context;
            _passService = passService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPasses()
        {
            var passes = await _passService.GetAllAsync();
            return Ok(passes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPassById(int id)
        {
            var pass = await _passService.GetByIdAsync(id);
            if (pass == null)
                return NotFound();
            return Ok(pass);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePass([FromBody] CreatePassDto newPass)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _passService.CreateAsync(newPass);
            return CreatedAtAction(nameof(GetPassById), new { id = created.TypeId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePass(int id, [FromBody] UpdatePassDto updatedPass)
        {
            var success = await _passService.UpdateAsync(id, updatedPass);
            if (!success)
                return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePass(int id)
        {
            var success = await _passService.DeleteAsync(id);
            if (!success)
                return NotFound();
            return NoContent();
        }
    }
}
