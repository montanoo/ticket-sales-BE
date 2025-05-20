using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TicketSystemAPI.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public decimal Amount { get; set; }

    public string Method { get; set; } = null!;

    public string? StripePaymentIntentId { get; set; }

    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore] // Prevent circular reference
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
