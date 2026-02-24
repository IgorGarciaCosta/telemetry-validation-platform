using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Api.Models;
using System.Text.Json;

namespace Api.Data;

public class DynamoDbEventRepository : IEventRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName = "Events"; // Vamos criar essa tabela no template.yaml

    public DynamoDbEventRepository(IAmazonDynamoDB dynamoDb)
    {
        _dynamoDb = dynamoDb;
    }

    public async Task<TelemetryEvent> CreateAsync(TelemetryEvent @event)
    {
        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = new Dictionary<string, AttributeValue>
            {
                { "Id", new AttributeValue { S = @event.Id.ToString() } },
                { "Type", new AttributeValue { S = @event.Type } },
                { "Timestamp", new AttributeValue { S = @event.Timestamp.ToString("O") } }, // ISO 8601
                { "Payload", new AttributeValue { S = @event.Payload ?? "" } }
            }
        };

        await _dynamoDb.PutItemAsync(request);
        return @event;
    }

    public async Task<TelemetryEvent?> GetByIdAsync(Guid id)
    {
        var request = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "Id", new AttributeValue { S = id.ToString() } }
            }
        };

        var response = await _dynamoDb.GetItemAsync(request);
        if (!response.IsItemSet) return null;

        var item = response.Item;
        return new TelemetryEvent
        {
            Id = Guid.Parse(item["Id"].S),
            Type = item["Type"].S,
            Timestamp = DateTimeOffset.Parse(item["Timestamp"].S),
            Payload = item.ContainsKey("Payload") ? item["Payload"].S : null
        };
    }

    public async Task<(IEnumerable<TelemetryEvent> Events, int TotalCount)> GetAllAsync(string? type, DateTimeOffset? from, DateTimeOffset? to, int page = 1, int pageSize = 10)
    {
        // OBS: DynamoDB não é bom para filtros complexos e paginação igual SQL.
        // Para este MVP, vamos fazer um Scan simples (não performático para milhões de dados, mas ok para teste).

        var request = new ScanRequest
        {
            TableName = _tableName,
            Limit = pageSize
        };

        // Adicionando filtro simples se houver Type
        if (!string.IsNullOrEmpty(type))
        {
            request.FilterExpression = "#t = :v_type";
            request.ExpressionAttributeNames = new Dictionary<string, string> { { "#t", "Type" } };
            request.ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":v_type", new AttributeValue { S = type } }
            };
        }

        var response = await _dynamoDb.ScanAsync(request);

        var events = response.Items.Select(item => new TelemetryEvent
        {
            Id = Guid.Parse(item["Id"].S),
            Type = item["Type"].S,
            Timestamp = DateTimeOffset.Parse(item["Timestamp"].S),
            Payload = item.ContainsKey("Payload") ? item["Payload"].S : null
        });

        return (events, response.Count.GetValueOrDefault());
    }
}
