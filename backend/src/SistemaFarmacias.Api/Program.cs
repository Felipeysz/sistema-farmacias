using DotNetEnv;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaFarmacias.Api.Auth;
using SistemaFarmacias.Application.Events;
using SistemaFarmacias.Application.Interfaces;
using SistemaFarmacias.Infrastructure.Messaging;
using SistemaFarmacias.Infrastructure.Persistence;
using SistemaFarmacias.Infrastructure.Repositories;

// Em CI/testes de integração não existe .env — Env.Load lançaria uma exceção
// e derrubaria o host antes mesmo de começar. Nesses ambientes as variáveis
// já vêm de outra forma (configuração injetada pelo WebApplicationFactory).
try
{
    var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", ".env");
    Env.Load(envPath);
}
catch (FileNotFoundException)
{
    // Sem .env disponível (CI, testes de integração) — segue com o que já
    // estiver em Environment/Configuration.
}

var builder = WebApplication.CreateBuilder(args);

var pgHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
var pgPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
var pgDb = Environment.GetEnvironmentVariable("POSTGRES_DB");
var pgUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
var pgPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

builder.Configuration["ConnectionStrings:DefaultConnection"] =
    $"Host={pgHost};Port={pgPort};Database={pgDb};Username={pgUser};Password={pgPassword}";

builder.Configuration["N8n:BackendApiKey"] = Environment.GetEnvironmentVariable("N8N_BACKEND_API_KEY");

builder.Services.AddOpenApi();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiKeyAuthAttribute>();
});

builder.Services.AddScoped<IFarmaciaRepository, FarmaciaRepository>();
builder.Services.AddScoped<IContatoRepository, ContatoRepository>();
builder.Services.AddScoped<IInteracaoRepository, InteracaoRepository>();
builder.Services.AddScoped<IReativacaoRepository, ReativacaoRepository>();
builder.Services.AddScoped<IVendaProcessadaRepository, VendaProcessadaRepository>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// LASDWAS-13: MediatR para eventos de domínio (VendaRegistradaEvent)
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<VendaRegistradaEvent>());

// LASDWAS-13: RabbitMQ (publisher síncrono no fluxo + consumer assíncrono em background)
var rabbitSettings = new RabbitMqSettings
{
    HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbitmq",
    Port = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out var p) ? p : 5672,
    VirtualHost = Environment.GetEnvironmentVariable("RABBITMQ_VHOST") ?? "/",
    UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "",
    Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? ""
};
builder.Services.AddSingleton(rabbitSettings);
builder.Services.AddSingleton<IN8nWebhookNotifier, N8nWebhookNotifier>();

builder.Services.AddHttpClient();

var vendaWebhookUrl = Environment.GetEnvironmentVariable("N8N_VENDA_WEBHOOK_URL")
    ?? "http://n8n:5678/webhook/venda-registrada";
builder.Services.AddSingleton(vendaWebhookUrl);
builder.Services.AddHostedService<N8nWebhookConsumerService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();

// Necessário para o WebApplicationFactory<Program> localizar o entry point
// nos testes de integração (LASDWAS-23).
public partial class Program { }
