namespace AuthService.Messages;

public class DoctorLoggedIn
{
    public int UserId { get; set; }
    
    public int DoctorId { get; set; }
    
    public string Name { get; set; }
}