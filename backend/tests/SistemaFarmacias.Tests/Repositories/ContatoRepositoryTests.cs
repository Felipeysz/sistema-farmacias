using SistemaFarmacias.Domain.Entities;
using SistemaFarmacias.Infrastructure.Repositories;

namespace SistemaFarmacias.Tests.IntegrationTests;

/// <summary>
/// Testes do ContatoRepository contra um Postgres real (Testcontainers), não
/// InMemory — necessário desde a LASDWAS-24, que passou a usar
/// ExecuteUpdateAsync em AtualizarAposVendaAsync (recurso que depende de
/// tradução para SQL real e não é suportado pelo provider InMemory).
/// </summary>
[Collection("Integration")]
public class ContatoRepositoryTests
{
    private readonly IntegrationTestFixture _fixture;

    public ContatoRepositoryTests(IntegrationTestFixture fixture) => _fixture = fixture;

    private async Task<Guid> SeedFarmaciaAsync()
    {
        await using var context = await _fixture.CreateDbContextAsync();
        var farmaciaId = Guid.NewGuid();
        context.Farmacias.Add(new Farmacia { Id = farmaciaId, Nome = "Farmácia Repository Teste" });
        await context.SaveChangesAsync();
        return farmaciaId;
    }

    [Fact]
    public async Task UpsertAsync_QuandoContatoNaoExiste_CriaNovoContatoAtivo()
    {
        // Arrange
        await using var context = await _fixture.CreateDbContextAsync();
        var repository = new ContatoRepository(context);
        var farmaciaId = await SeedFarmaciaAsync();
        var telefone = $"55119{Random.Shared.Next(10000000, 99999999)}";

        // Act
        var contato = await repository.UpsertAsync(farmaciaId, telefone, "Cliente Teste");

        // Assert
        Assert.NotEqual(Guid.Empty, contato.Id);
        Assert.Equal(farmaciaId, contato.FarmaciaId);
        Assert.Equal(telefone, contato.Telefone);
        Assert.Equal("Cliente Teste", contato.Nome);
        Assert.Equal(StatusContato.Ativo, contato.Status);
    }

    [Fact]
    public async Task UpsertAsync_QuandoContatoJaExiste_AtualizaNomeSemDuplicar()
    {
        // Arrange
        await using var context = await _fixture.CreateDbContextAsync();
        var repository = new ContatoRepository(context);
        var farmaciaId = await SeedFarmaciaAsync();
        var telefone = $"55119{Random.Shared.Next(10000000, 99999999)}";
        var primeiro = await repository.UpsertAsync(farmaciaId, telefone, "Nome Antigo");

        // Act
        var segundo = await repository.UpsertAsync(farmaciaId, telefone, "Nome Novo");

        // Assert
        Assert.Equal(primeiro.Id, segundo.Id);
        Assert.Equal("Nome Novo", segundo.Nome);
    }

    [Fact]
    public async Task UpsertAsync_QuandoNomeNaoInformado_MantemNomeExistente()
    {
        // Arrange
        await using var context = await _fixture.CreateDbContextAsync();
        var repository = new ContatoRepository(context);
        var farmaciaId = await SeedFarmaciaAsync();
        var telefone = $"55119{Random.Shared.Next(10000000, 99999999)}";
        await repository.UpsertAsync(farmaciaId, telefone, "Nome Original");

        // Act
        var resultado = await repository.UpsertAsync(farmaciaId, telefone, null);

        // Assert
        Assert.Equal("Nome Original", resultado.Nome);
    }

    [Fact]
    public async Task AtualizarAposVendaAsync_SomaValorAoInvesDeSobrescrever()
    {
        // Arrange
        await using var context = await _fixture.CreateDbContextAsync();
        var repository = new ContatoRepository(context);
        var farmaciaId = await SeedFarmaciaAsync();
        var telefone = $"55119{Random.Shared.Next(10000000, 99999999)}";
        var contato = await repository.UpsertAsync(farmaciaId, telefone, "Cliente");
        await repository.AtualizarAposVendaAsync(contato.Id, farmaciaId, DateTime.UtcNow, 50m);

        // Act — segunda compra
        var resultado = await repository.AtualizarAposVendaAsync(contato.Id, farmaciaId, DateTime.UtcNow, 30m);

        // Assert
        Assert.Equal(80m, resultado!.TotalGasto);
    }

    [Fact]
    public async Task AtualizarAposVendaAsync_QuandoContatoEstaInativo_ReativaAutomaticamente()
    {
        // Arrange
        await using var context = await _fixture.CreateDbContextAsync();
        var repository = new ContatoRepository(context);
        var farmaciaId = await SeedFarmaciaAsync();
        var telefone = $"55119{Random.Shared.Next(10000000, 99999999)}";
        var contato = await repository.UpsertAsync(farmaciaId, telefone, "Cliente");

        var entidade = await context.Contatos.FindAsync(contato.Id);
        entidade!.Status = StatusContato.Inativo;
        await context.SaveChangesAsync();

        // Act
        var resultado = await repository.AtualizarAposVendaAsync(contato.Id, farmaciaId, DateTime.UtcNow, 10m);

        // Assert
        Assert.Equal(StatusContato.Ativo, resultado!.Status);
    }

    [Fact]
    public async Task AtualizarAposVendaAsync_QuandoContatoDeOutraFarmacia_RetornaNull()
    {
        // Arrange — isolamento entre tenants
        await using var context = await _fixture.CreateDbContextAsync();
        var repository = new ContatoRepository(context);
        var farmaciaA = await SeedFarmaciaAsync();
        var farmaciaB = Guid.NewGuid(); // não precisa existir: só usada no filtro, nunca inserida
        var telefone = $"55119{Random.Shared.Next(10000000, 99999999)}";
        var contato = await repository.UpsertAsync(farmaciaA, telefone, "Cliente");

        // Act
        var resultado = await repository.AtualizarAposVendaAsync(contato.Id, farmaciaB, DateTime.UtcNow, 10m);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task GetInativosAsync_RetornaApenasContatosInativosDaFarmaciaCorreta()
    {
        // Arrange
        await using var context = await _fixture.CreateDbContextAsync();
        var repository = new ContatoRepository(context);
        var farmaciaA = await SeedFarmaciaAsync();
        var farmaciaB = await SeedFarmaciaAsync();

        var telefoneAtivo = $"55119{Random.Shared.Next(10000000, 99999999)}";
        await repository.UpsertAsync(farmaciaA, telefoneAtivo, "Ativo A");

        var telefoneInativoA = $"55119{Random.Shared.Next(10000000, 99999999)}";
        var inativoA = await repository.UpsertAsync(farmaciaA, telefoneInativoA, "Inativo A");

        var telefoneInativoB = $"55119{Random.Shared.Next(10000000, 99999999)}";
        var inativoB = await repository.UpsertAsync(farmaciaB, telefoneInativoB, "Inativo B (outra farmácia)");

        var entidadeA = await context.Contatos.FindAsync(inativoA.Id);
        entidadeA!.Status = StatusContato.Inativo;
        var entidadeB = await context.Contatos.FindAsync(inativoB.Id);
        entidadeB!.Status = StatusContato.Inativo;
        await context.SaveChangesAsync();

        // Act
        var resultado = await repository.GetInativosAsync(farmaciaA);

        // Assert
        Assert.Single(resultado);
        Assert.Equal("Inativo A", resultado[0].Nome);
    }
}
