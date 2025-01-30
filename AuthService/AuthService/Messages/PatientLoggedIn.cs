namespace AuthService.Messages;

public class PatientLoggedIn
{
    public int UserId { get; set; }
    
    public int PatientId { get; set; }
    
    public string Name { get; set; }
}