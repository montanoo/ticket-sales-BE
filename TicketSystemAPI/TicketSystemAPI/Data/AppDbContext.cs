using Microsoft.EntityFrameworkCore;
using TicketSystemAPI.Models;

namespace TicketSystemAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Ticket> Tickets { get; set; }
    }
}
