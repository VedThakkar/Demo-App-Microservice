namespace AuthService.Messages;

public class UserRegistered
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public DateTime RegisteredAt { get; set; }
    
    public string Username { get; set; }
}