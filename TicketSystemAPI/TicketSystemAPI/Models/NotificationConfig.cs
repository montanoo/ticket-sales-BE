namespace TicketSystemAPI.Models
{
    public class NotificationConfig
    {
        public int Id { get; set; } = 1; // Singleton
        public string EmailSubjectTemplate { get; set; } = string.Empty;
        public string EmailBodyTemplate { get; set; } = string.Empty;
        public bool EnableEmailNotifications { get; set; } = true;
    }
}
