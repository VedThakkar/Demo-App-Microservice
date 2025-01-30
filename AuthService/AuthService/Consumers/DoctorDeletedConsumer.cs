using AuthService.Messages;
using AuthService.Services.IServices;
using MassTransit;
using Serilog;

namespace AuthService.Consumers;

public class DoctorDeletedConsumer : IConsumer<DoctorDeleted>
{
    private readonly IUserService _userService;

    public DoctorDeletedConsumer(IUserService userService)
    {
        _userService = userService;
    }
    public async Task Consume(ConsumeContext<DoctorDeleted> context)
    {
        var message = context.Message;

        await _userService.DeleteUserById(message.UserId);
        
        Log.Information($"Doctor Deleted with UserId {message.UserId}, Doctor Id {message.DoctorId} and Name {message.Name} ");

        await Task.CompletedTask;
    }
}