using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSystemAPI.Data;
using TicketSystemAPI.Models;
using TicketSystemAPI.DTO;
using TicketSystemAPI.Helpers;

namespace TicketSystemAPI.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/sales")]
    public class AdminSalesController : ControllerBase
    {
        private readonly TicketSystemContext _context;

        public AdminSalesController(TicketSystemContext context)
        {
            _context = context;
        }












    }
}
