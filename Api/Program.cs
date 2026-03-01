using Api.Data;
using Microsoft.EntityFrameworkCore;
using Api.Services;
using Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Amazon.Lambda.AspNetCoreServer.Hosting;
using Amazon.DynamoDBv2;
using Amazon.SQS;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

// 1. Adiciona suporte a Controllers (essencial para o EventsController)
builder.Services.AddControllers();
var isRunningInLambda = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME"));

if (isRunningInLambda)
{
    // Configuração para NUVEM (DynamoDB)
    builder.Services.AddAWSService<IAmazonDynamoDB>();
    builder.Services.AddScoped<IEventRepository, DynamoDbEventRepository>();
    builder.Services.AddAWSService<IAmazonSQS>();
    builder.Services.AddScoped<IMessageQueueService, SqsService>();
    Console.WriteLine("--> Usando DynamoDB + SQS (Cloud Mode)");
}
else
{
    // Configuração para LOCAL (Postgres)
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
    );
    builder.Services.AddScoped<IEventRepository, PostgresEventRepository>();
    builder.Services.AddScoped<IMessageQueueService, MockMessageQueueService>();
    Console.WriteLine("--> Usando Postgres (Local Mode)");
}
// Registrando o Serviço
// Scoped = Cria um novo serviço para cada requisição HTTP (ideal para Web APIs)
builder.Services.AddScoped<ITelemetryService, TelemetryService>();
// 2. Adiciona suporte ao Swagger (para documentação e teste)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- CONFIGURAÇÃO DO JWT ---
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key is not configured"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});
// ---------------------------


var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>(); // Adiciona o Middleware Global de Tratamento de Erros

// 3. Configura o Swagger visual
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

// (Opcional) Desabilita redirecionamento HTTPS por enquanto para evitar avisos de porta
app.UseHttpsRedirection();

app.UseAuthentication(); // Quem é você?
app.UseAuthorization();  // O que você pode fazer?
// 4. Mapeia os Controllers para que a API encontre suas rotas
app.MapControllers();

// 5. Um endpoint simples de saúde (Health Check)
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
public partial class Program { }
