using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SistemaFarmacias.Application.Interfaces;
using SistemaFarmacias.Infrastructure.Messaging;
using SistemaFarmacias.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace SistemaFarmacias.Tests.IntegrationTests;

/// <summary>
/// Sobe um container Postgres real (via Testcontainers) uma única vez para
/// toda a suíte de testes de integração, aplica as migrations do EF Core nele,
/// e monta um WebApplicationFactory da API apontando para esse banco.
///
/// Compartilhada entre classes de teste via [Collection("Integration")] —
/// subir um container novo por classe seria lento demais.
/// </summary>
public class IntegrationTestFixture : IAsyncLifetime
{
    public const string TestApiKey = "test-api-key-integration";

    private PostgreSqlContainer _postgresContainer = null!;
    private WebApplicationFactory<Program> _factory = null!;

    public HttpClient CreateAuthenticatedClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", TestApiKey);
        return client;
    }

    public HttpClient CreateUnauthenticatedClient() => _factory.CreateClient();

    public async Task<AppDbContext> CreateDbContextAsync()
    {
        var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await Task.CompletedTask;
        return context;
    }

    public async Task InitializeAsync()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("sistema_farmacias_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .Build();

        await _postgresContainer.StartAsync();

        var connectionString = _postgresContainer.GetConnectionString();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["N8n:BackendApiKey"] = TestApiKey,
                        ["ConnectionStrings:DefaultConnection"] = connectionString
                    });
                });

                builder.ConfigureServices(services =>
                {
                    // Troca o DbContext para apontar para o Postgres do container
                    var dbContextDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (dbContextDescriptor is not null)
                        services.Remove(dbContextDescriptor);

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseNpgsql(connectionString));

                    // Remove o consumer de RabbitMQ — não há fila disponível no
                    // ambiente de teste, e não é isso que esta suíte cobre.
                    var hostedServiceDescriptor = services.SingleOrDefault(d =>
                        d.ServiceType == typeof(IHostedService) &&
                        d.ImplementationType == typeof(N8nWebhookConsumerService));
                    if (hostedServiceDescriptor is not null)
                        services.Remove(hostedServiceDescriptor);

                    // Substitui o notifier real (que tentaria conectar de
                    // verdade ao RabbitMQ) por um no-op — o fluxo de vendas
                    // testado aqui é sobre idempotência/concorrência do
                    // contato, não sobre a entrega da notificação ao n8n.
                    var notifierDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IN8nWebhookNotifier));
                    if (notifierDescriptor is not null)
                        services.Remove(notifierDescriptor);
                    services.AddSingleton<IN8nWebhookNotifier, NoOpN8nWebhookNotifier>();
                });
            });

        // Aplica as migrations reais do projeto no banco do container —
        // mais fiel ao comportamento de produção do que EnsureCreated().
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }
}

[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
{
}
