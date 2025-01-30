using MassTransit.Futures.Contracts;

namespace AuthService.Messages;

public class PatientDeleted
{
    public int UserId { get; set; }
    
    public int PatientId { get; set; }
    
    public string Name { get; set; }
}