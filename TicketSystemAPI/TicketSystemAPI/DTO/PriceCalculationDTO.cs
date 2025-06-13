namespace TicketSystemAPI.DTO
{
    public class PriceCalculationRequest
    {
        public int TypeId { get; set; }
        public string? DiscountCode { get; set; }
    }

    public class PriceCalculationResponse
    {
        public decimal BasePrice { get; set; }
        public decimal FinalPrice { get; set; }
        public decimal? DiscountApplied { get; set; }
        public bool PromoValid { get; set; }
    }
}
