using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.DynamoDBv2;
using System.Text.Json;
using Api.Models;


[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace Api.Worker;

public class EventProcessor
{

    private readonly MetricsRepository _metricsRepo;

    public EventProcessor()
    {
        // Inicializa o cliente DynamoDB e o Repositório
        var dbClient = new AmazonDynamoDBClient();
        _metricsRepo = new MetricsRepository(dbClient);
    }


    // Este é o método que a AWS vai chamar quando tiver mensagens na fila
    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        foreach (var message in evnt.Records)
        {
            await ProcessMessageAsync(message, context);
        }
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        try
        {
            context.Logger.LogInformation($"[WORKER] Processando: {message.Body}");

            // 1. Deserializa a mensagem para o nosso objeto de evento
            var telemetryEvent = JsonSerializer.Deserialize<TelemetryEvent>(message.Body);

            if (telemetryEvent != null && !string.IsNullOrEmpty(telemetryEvent.Type))
            {
                // 2. Incrementa o contador no DynamoDB
                await _metricsRepo.IncrementMetricAsync(telemetryEvent.Type, telemetryEvent.Timestamp);

                context.Logger.LogInformation($"[WORKER] Métrica atualizada para: {telemetryEvent.Type}");
            }
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"[WORKER ERROR] Falha ao processar mensagem {message.MessageId}: {ex.Message}");
            // Em produção, aqui você jogaria para uma Dead Letter Queue (DLQ)
        }
    }
}
