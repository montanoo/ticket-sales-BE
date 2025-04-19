namespace TicketSystemAPI.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Variant { get; set; } // e.g., "1-day", "1-week", "3-day-limited"
        public decimal Price { get; set; }
        public int? MaxRides { get; set; } // null for unlimited
        public string? DiscountCode { get; set; }
        public DateTime PurchaseDate { get; set; }

        //public int Id { get; set; }
        //public string Type { get; set; }
        //public bool IsUnlimited { get; set; }
        //public int? RideLimit { get; set; }
        //public decimal Price { get; set; }
    }
}
