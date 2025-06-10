namespace TicketSystemAPI.Models
{
    public class Promotion
    {
        public int Id { get; set; }
        public string PromoCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal DiscountPercentage { get; set; }
    }
}
