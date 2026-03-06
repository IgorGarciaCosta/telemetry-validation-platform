using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Api.Worker;

public class MetricsRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;

    public MetricsRepository(IAmazonDynamoDB dynamoDb)
    {
        _dynamoDb = dynamoDb;
        _tableName = Environment.GetEnvironmentVariable("METRICS_TABLE_NAME") ?? "Metrics";
    }

    public async Task IncrementMetricAsync(string metricType, DateTimeOffset date)
    {
        var pk = $"METRIC#{metricType}"; // Ex: METRIC#login
        var sk = $"DATE#{date:yyyy-MM-dd}"; // Ex: DATE#2026-03-01

        var request = new UpdateItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "PK", new AttributeValue { S = pk } },
                { "SK", new AttributeValue { S = sk } }
            },
            // Expressão mágica: "Set Count = Count + 1" (se não existir, começa do 0)
            UpdateExpression = "ADD #count :inc",
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#count", "Count" }
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":inc", new AttributeValue { N = "1" } }
            }
        };

        await _dynamoDb.UpdateItemAsync(request);
        Console.WriteLine($"[METRICS] Incrementado: {metricType} em {date:yyyy-MM-dd}");
    }
}
