namespace TicketSystemAPI.Models
{
    public class TicketPurchaseRequest
    {
        public string Variant { get; set; }
        public string? DiscountCode { get; set; }
    }
}
