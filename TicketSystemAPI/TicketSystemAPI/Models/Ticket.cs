using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TicketSystemAPI.Models;

public partial class Ticket
{
    public int TicketId { get; set; }

    public int TypeId { get; set; }

    public DateTime? PurchaseTime { get; set; }

    public DateTime? ExpirationTime { get; set; }

    public uint? RidesTaken { get; set; }

    public uint? RideLimit { get; set; }

    public decimal? Price { get; set; }

    public string? DiscountCode { get; set; }

    public int UserId { get; set; }

    public int? PaymentId { get; set; }

    public string? Status { get; set; } // Reserved, Paid, Expired
    public DateTime? ReservedAt { get; set; }


    public virtual Payment Payment { get; set; } = null!;

    public virtual Tickettype Type { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    [JsonIgnore] // Prevent circular reference
    public virtual ICollection<TicketEntryHistory> EntryHistory { get; set; } = new List<TicketEntryHistory>();


    public bool IsValid()
    {
        if (Status != "Paid")
            return false;

        if (RideLimit.HasValue && RidesTaken.HasValue)
        {
            if (RidesTaken >= RideLimit)
                return false;         
        }

        if (ExpirationTime.HasValue && ExpirationTime < DateTime.UtcNow)
            return false;

        return true;
    }
}
