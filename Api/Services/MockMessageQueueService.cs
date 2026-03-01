using Api.Services;

namespace Api.Services;

public class MockMessageQueueService : IMessageQueueService
{
    private readonly ILogger<MockMessageQueueService> _logger;

    public MockMessageQueueService(ILogger<MockMessageQueueService> logger)
    {
        _logger = logger;
    }

    public Task SendMessageAsync(string messageBody)
    {
        // Em vez de mandar pra AWS, só escrevemos no console do Docker/Terminal
        _logger.LogInformation("--> [MOCK LOCAL] Mensagem enviada para fila imaginária: {Message}", messageBody);
        return Task.CompletedTask;
    }
}
