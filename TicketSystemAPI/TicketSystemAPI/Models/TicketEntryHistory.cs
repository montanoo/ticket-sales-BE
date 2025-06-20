using System;

namespace TicketSystemAPI.Models;

public class TicketEntryHistory
{
    public int Id { get; set; }

    public int TicketId { get; set; }

    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;

    public virtual Ticket Ticket { get; set; } = null!;
}