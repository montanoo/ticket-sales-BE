namespace TicketSystemAPI.DTO
{
    public class UpdateProfileDto
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
    }

    public class ChangePasswordDto
    {
        public string OldPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }

}
