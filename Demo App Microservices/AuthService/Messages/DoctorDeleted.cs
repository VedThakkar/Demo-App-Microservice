namespace AuthService.Messages;

public class DoctorDeleted
{
    public int UserId { get; set; }
    
    public int DoctorId { get; set; }
    
    public string Name { get; set; }
}