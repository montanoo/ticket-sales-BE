using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSystemAPI.Data;
using TicketSystemAPI.Models;
using TicketSystemAPI.DTO;
using TicketSystemAPI.Helpers;

namespace TicketSystemAPI.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/config")]
    public class AdminConfigController : ControllerBase
    {
        private readonly TicketSystemContext _context;

        public AdminConfigController(TicketSystemContext context)
        {
            _context = context;
        }











     

    }
}
