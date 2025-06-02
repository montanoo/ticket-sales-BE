using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TicketSystemAPI.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Role { get; set; } = "User";  // default

    [JsonIgnore] // Prevent circular reference
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
