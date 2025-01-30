using AuthService.Messages;
using MassTransit;
using PatientService.Models;
using PatientService.Services.IServices;

namespace PatientService.Consumers;

public class UserRegisteredConsumer : IConsumer<UserRegistered>
{
    private readonly IPatientService _patientService;

    public UserRegisteredConsumer(IPatientService patientService)
    {
        _patientService = patientService;
    }

    public async Task Consume(ConsumeContext<UserRegistered> context)
    {
        var message = context.Message;
        Console.WriteLine($"User {message.Email} registered with Role {message.Role}");

        var patient = new Patient
        {
            UserId = context.Message.UserId,
            Name = context.Message.Username,
            DateOfBirth = null,
            Gender = null
        };

        await _patientService.AddPatientAsync(patient);
        
        await Task.CompletedTask;
    }
}