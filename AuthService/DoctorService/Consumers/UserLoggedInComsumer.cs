using AuthService.Messages;
using DoctorService.AuthProvider.IAuthProvider;
using DoctorService.Services.IServices;
using MassTransit;
using Serilog;

namespace DoctorService.Consumers;

public class UserLoggedInConsumer : IConsumer<UserLoggedIn>
{
    private readonly IBus _bus;
    private readonly IDoctorService _doctorService;

    public UserLoggedInConsumer(IDoctorService doctorService, IBus bus)
    {
        _doctorService = doctorService;
        _bus = bus;
    }

    public async Task Consume(ConsumeContext<UserLoggedIn> context)
    {
        var message = context.Message;
        Log.Information($"User {message.Email} logged in. Access Token: {message.AccessToken} with Role {message.Role}, Refresh Token: {message.RefreshToken}");
        
        var doctor = await _doctorService.GetByUserIdAsync(message.UserId);
        await _bus.Publish(new DoctorLoggedIn
        {
            UserId = message.UserId,
            DoctorId = doctor.Id,
            Name = doctor.Name
        });

        await Task.CompletedTask;
    }
}
