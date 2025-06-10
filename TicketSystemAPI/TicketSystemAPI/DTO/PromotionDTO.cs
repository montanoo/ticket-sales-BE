namespace TicketSystemAPI.DTO
{
    public class PromotionCreateDto
    {
        public string PromoCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal DiscountPercentage { get; set; }
    }
    public class PromotionUpdateDto
    {
        public string PromoCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal DiscountPercentage { get; set; }
    }

}
