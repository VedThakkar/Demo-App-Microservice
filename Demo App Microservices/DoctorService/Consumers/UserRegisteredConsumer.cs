using AuthService.Messages;
using DoctorService.Models;
using DoctorService.Services.IServices;
using MassTransit;

namespace DoctorService.Consumers;

public class UserRegisteredConsumer : IConsumer<UserRegistered>
{
    private readonly IDoctorService _doctorService;

    public UserRegisteredConsumer(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    public async Task Consume(ConsumeContext<UserRegistered> context)
    {
        var message = context.Message;
        Console.WriteLine($"User {message.Email} registered with Role {message.Role}");

        var doctor = new Doctor
        {
            UserId = context.Message.UserId,
            Name = context.Message.Username
        };

        await _doctorService.AddDoctorAsync(doctor);
        
        await Task.CompletedTask;
    }
}