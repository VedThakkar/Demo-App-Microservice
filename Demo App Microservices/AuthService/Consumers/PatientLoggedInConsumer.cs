using System.Collections.Concurrent;
using AuthService.Messages;
using MassTransit;
using Serilog;

namespace AuthService.Consumers;

public class PatientLoggedInConsumer : IConsumer<PatientLoggedIn>
{
    // Use a thread-safe in-memory store (ConcurrentDictionary) with UserId as key
    private static ConcurrentDictionary<int, PatientDetails> _patientStore = new ConcurrentDictionary<int, PatientDetails>();
    
    public async Task Consume(ConsumeContext<PatientLoggedIn> context)
    {
        var message = context.Message;
        
        Log.Information($"Patient Logged in with Id {message.PatientId} and Name {message.Name}");
        
        // Store the patient details temporarily based on UserId
        var patientDetails = new PatientDetails
        {
            UserId = message.UserId,
            PatientId = message.PatientId,
            Name = message.Name,
        };

        // Store in the in-memory dictionary with UserId as key
        _patientStore[message.UserId] = patientDetails;

        await Task.CompletedTask;
    }
    
    // Method to retrieve patient details using UserId as key
    public static PatientDetails GetPatientDetails(int userId)
    {
        _patientStore.TryGetValue(userId, out var patientDetails);
        return patientDetails;
    }
}

// Define the patient details structure
public class PatientDetails
{
    public int UserId { get; set; }
    public int PatientId { get; set; }
    public string Name { get; set; }
}