using Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Adiciona suporte a Controllers (essencial para o seu EventsController)
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// 2. Adiciona suporte ao Swagger (para documentação e teste)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 3. Configura o Swagger visual
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// (Opcional) Desabilita redirecionamento HTTPS por enquanto para evitar avisos de porta
// app.UseHttpsRedirection();

// 4. Mapeia os Controllers para que a API encontre suas rotas
app.MapControllers();

// 5. Um endpoint simples de saúde (Health Check)
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
