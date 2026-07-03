using DotNetEnv;
using Microsoft.EntityFrameworkCore;
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
builder.Services.AddControllers();
builder.Services.AddScoped<IFarmaciaRepository, FarmaciaRepository>();
builder.Services.AddScoped<IContatoRepository, ContatoRepository>();
builder.Services.AddScoped<IInteracaoRepository, InteracaoRepository>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();