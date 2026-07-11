using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using SistemaFarmacias.Api.Auth;
using SistemaFarmacias.Application.Interfaces;
using SistemaFarmacias.Infrastructure.Persistence;
using SistemaFarmacias.Infrastructure.Repositories;

// Carrega o .env da raiz do monorepo (backend/src/SistemaFarmacias.Api -> 3 níveis acima)
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", ".env");
Env.Load(envPath);

var builder = WebApplication.CreateBuilder(args);

// Monta a connection string a partir das variáveis do .env
var pgHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
var pgPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
var pgDb = Environment.GetEnvironmentVariable("POSTGRES_DB");
var pgUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
var pgPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

builder.Configuration["ConnectionStrings:DefaultConnection"] =
    $"Host={pgHost};Port={pgPort};Database={pgDb};Username={pgUser};Password={pgPassword}";

builder.Configuration["N8n:BackendApiKey"] = Environment.GetEnvironmentVariable("N8N_BACKEND_API_KEY");

builder.Services.AddOpenApi();

// LASDWAS-12: ApiKeyAuthAttribute aplicado globalmente a todos os controllers,
// em vez de exigir o atributo [ApiKeyAuth] repetido manualmente em cada um.
// Como todas as rotas do projeto vivem sob /api/n8n/*, isso protege
// automaticamente qualquer endpoint novo, presente ou futuro.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiKeyAuthAttribute>();
});

builder.Services.AddScoped<IFarmaciaRepository, FarmaciaRepository>();
builder.Services.AddScoped<IContatoRepository, ContatoRepository>();
builder.Services.AddScoped<IInteracaoRepository, InteracaoRepository>();
builder.Services.AddScoped<IReativacaoRepository, ReativacaoRepository>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();
