using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TicketSystemAPI.Models;

public partial class Tickettype
{
    public int TypeId { get; set; }

    public string Name { get; set; } = null!;

    public int? BaseDurationDays { get; set; }

    public int? BaseRideLimit { get; set; }

    public decimal? BasePrice { get; set; }

    [JsonIgnore] // Ignore the Tickets property when serializing TicketType
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
