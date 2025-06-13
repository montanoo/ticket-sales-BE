namespace TicketSystemAPI.DTO
{
    public class PassDto
    {
        public int TypeId { get; set; }
        public string Name { get; set; } = null!;
        public int? BaseDurationDays { get; set; }
        public int? BaseRideLimit { get; set; }
        public decimal? BasePrice { get; set; }
    }

    public class CreatePassDto
    {
        public string Name { get; set; } = null!;
        public int? BaseDurationDays { get; set; }
        public int? BaseRideLimit { get; set; }
        public decimal? BasePrice { get; set; }
    }

    public class UpdatePassDto
    {
        public string Name { get; set; } = null!;
        public int? BaseDurationDays { get; set; }
        public int? BaseRideLimit { get; set; }
        public decimal? BasePrice { get; set; }
    }

    public class UserDto
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
    }

    public class UpdatePermissionsDto
    {
        public string Role { get; set; }
    }

    public class ResetPasswordDto
    {
        public string NewPassword { get; set; }
    }

}
