namespace AuthService.Messages;

public class UserLoggedIn
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public DateTime LoginTime { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}