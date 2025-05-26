using System;
using Microsoft.EntityFrameworkCore;
using TicketSystemAPI.Data;

public class TicketExpirationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public TicketExpirationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TicketSystemContext>();

                var expirationThreshold = DateTime.UtcNow.AddMinutes(-10);

                var expiredTickets = await dbContext.Tickets
                    .Where(t => t.Status == "Reserved" && t.ReservedAt < expirationThreshold && t.PaymentId == null)
                    .ToListAsync();

                foreach (var ticket in expiredTickets)
                {
                    ticket.Status = "Expired";
                    Console.WriteLine($"Ticket ID {ticket.TicketId} expired due to payment timeout.");
                }

                await dbContext.SaveChangesAsync();
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // check every minute
        }
    }
}
