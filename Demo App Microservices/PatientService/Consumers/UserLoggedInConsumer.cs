using AuthService.Messages;
using MassTransit;
using PatientService.AuthProvider.IAuthProvider;
using PatientService.Services.IServices;
using Serilog;

namespace PatientService.Consumers;

public class UserLoggedInConsumer : IConsumer<UserLoggedIn>
{
    private readonly IBus _bus;
    private readonly IPatientService _patientService;

    public UserLoggedInConsumer(IBus bus, IPatientService patientService)
    {
        _bus = bus;
        _patientService = patientService;
    }

    public async Task Consume(ConsumeContext<UserLoggedIn> context)
    {
        var message = context.Message;
        Log.Information($"Patient {message.Email} logged in. Access Token: {message.AccessToken} with Role {message.Role}, Refresh Token: {message.RefreshToken}");

        var patient = await _patientService.GetByUserIdAsync(message.UserId);
        await _bus.Publish(new PatientLoggedIn
        {
            UserId = message.UserId,
            PatientId = patient.Id,
            Name = patient.Name
        });

        await Task.CompletedTask;
    }
}
