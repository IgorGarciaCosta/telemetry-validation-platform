using Api.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
#pragma warning disable CS0618

namespace Api.IntegrationTests;

// Essa classe cria uma cópia da sua API, mas troca o Banco de Dados
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 1. Encontra a configuração do Postgres antigo
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            // 2. Remove ela (não queremos usar o Postgres real nos testes)
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // 3. Adiciona um Banco em Memória novinho para o teste
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });

            services.AddAuthentication("TestScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", null);
        });
    }
}

// 🔑 Handler de autenticação fake - sempre autoriza
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user"),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Role, "Admin") // 🔑 Adiciona role Admin
        };

        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new System.Security.Principal.GenericPrincipal(identity, new[] { "Admin" });
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
