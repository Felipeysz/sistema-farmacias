using Microsoft.EntityFrameworkCore;
using SistemaFarmacias.Infrastructure.Persistence;

namespace SistemaFarmacias.Tests.TestHelpers;

public static class TestDbContextFactory
{
    /// <summary>
    /// Cria um AppDbContext usando o provider InMemory do EF Core, com um
    /// nome de banco único por chamada — garante isolamento total entre
    /// testes, sem precisar de um Postgres real rodando.
    /// </summary>
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
