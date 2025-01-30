using AuthService.Messages;
using AuthService.Services.IServices;
using MassTransit;
using Serilog;

namespace AuthService.Consumers;

public class PatientDeletedConsumer : IConsumer<PatientDeleted>
{
    private readonly IUserService _userService;

    public PatientDeletedConsumer(IUserService userService)
    {
        _userService = userService;
    }

    public async Task Consume(ConsumeContext<PatientDeleted> context)
    {
        var message = context.Message;

        await _userService.DeleteUserById(message.UserId);
        
        Log.Information($"Patient Deleted with UserId {message.UserId}, Patient Id {message.PatientId} and Name {message.Name} ");

        await Task.CompletedTask;
    }
}