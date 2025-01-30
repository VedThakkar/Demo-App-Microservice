using AuthService.Messages;
using MassTransit;
using Serilog;

namespace AuthService.Consumers;

public class DoctorLoggedInConsumer : IConsumer<DoctorLoggedIn>
{
    public async Task Consume(ConsumeContext<DoctorLoggedIn> context)
    {
        var message = context.Message;
        
        Log.Information($"Doctor Logged in with Id {message.DoctorId} and Name {message.Name}");

        await Task.CompletedTask;
    }
}