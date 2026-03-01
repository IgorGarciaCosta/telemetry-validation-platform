using Amazon.SQS;
using Amazon.SQS.Model;

namespace Api.Services;

public class SqsService : IMessageQueueService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly string _queueUrl;

    public SqsService(IAmazonSQS sqsClient, IConfiguration configuration)
    {
        _sqsClient = sqsClient;
        // Pega a URL que definimos no template.yaml
        _queueUrl = configuration["SQS_QUEUE_URL"] ?? "";
    }

    public async Task SendMessageAsync(string messageBody)
    {
        if (string.IsNullOrEmpty(_queueUrl)) return; // Se não tiver fila (local), ignora

        var request = new SendMessageRequest
        {
            QueueUrl = _queueUrl,
            MessageBody = messageBody
        };

        await _sqsClient.SendMessageAsync(request);
    }
}
