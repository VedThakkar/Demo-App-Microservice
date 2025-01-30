using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AuthService.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public int RoleId { get; set; }

    public virtual ICollection<Refreshtoken> Refreshtokens { get; set; } = new List<Refreshtoken>();
    
    [JsonIgnore]
    public virtual Role Role { get; set; } = null!;
}
