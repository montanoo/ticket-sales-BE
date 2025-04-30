namespace TicketSystemAPI.Models
{
    public class TicketPurchaseRequest
    {
        public int TypeId { get; set; }
        public int UserId { get; set; }
        public string? DiscountCode { get; set; } // Optional discount
    }
}
