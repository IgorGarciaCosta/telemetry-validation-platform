using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;


[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace Api.Worker;

public class EventProcessor
{
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
        context.Logger.LogInformation($"[WORKER] Processando mensagem ID: {message.MessageId}");
        context.Logger.LogInformation($"[WORKER] Conteúdo: {message.Body}");

        // AQUI ENTRARIA A LÓGICA PESADA (Cálculo de métricas, envio de email, etc)
        // Por enquanto, vamos simular um trabalho de 1 segundo
        await Task.Delay(1000);

        context.Logger.LogInformation($"[WORKER] Mensagem {message.MessageId} processada com sucesso!");
    }
}
