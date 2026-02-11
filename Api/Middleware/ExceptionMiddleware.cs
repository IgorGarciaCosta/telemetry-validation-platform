using System.Net;
using System.Text.Json;

namespace Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // Este método é chamado em TODA requisição
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);// Tenta passar a requisição para frente (Controller)
        }
        catch (Exception ex)
        {
            // Se algo explodiu lá dentro, caímos aqui
            _logger.LogError(ex, "Algo deu errado: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }

    }

    public static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        // Definimos o status code baseado no tipo do erro
        var statusCode = exception switch
        {
            // Se for erro de argumento (validação), é culpa do usuário (400)
            ArgumentException => (int)HttpStatusCode.BadRequest,
            // Qualquer outra coisa é culpa nossa (500)
            _ => (int)HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = statusCode;

        var repsonse = new
        {
            status = statusCode,
            error = exception.Message,
            details = "Erro tratado pelo Middleware Global"
        };

        var json = JsonSerializer.Serialize(repsonse);
        return context.Response.WriteAsync(json);
    }

}