namespace Api.Services;

public interface IMessageQueueService
{
    Task SendMessageAsync(string messageBody);
}
