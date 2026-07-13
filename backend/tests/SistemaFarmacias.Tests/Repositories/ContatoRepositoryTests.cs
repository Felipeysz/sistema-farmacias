using SistemaFarmacias.Domain.Entities;
using SistemaFarmacias.Infrastructure.Repositories;
using SistemaFarmacias.Tests.TestHelpers;

namespace SistemaFarmacias.Tests.Repositories;

public class ContatoRepositoryTests
{
    [Fact]
    public async Task UpsertAsync_QuandoContatoNaoExiste_CriaNovoContatoAtivo()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var repository = new ContatoRepository(context);
        var farmaciaId = Guid.NewGuid();

        // Act
        var contato = await repository.UpsertAsync(farmaciaId, "5511988887777", "Cliente Teste");

        // Assert
        Assert.NotEqual(Guid.Empty, contato.Id);
        Assert.Equal(farmaciaId, contato.FarmaciaId);
        Assert.Equal("5511988887777", contato.Telefone);
        Assert.Equal("Cliente Teste", contato.Nome);
        Assert.Equal(StatusContato.Ativo, contato.Status);
    }

    [Fact]
    public async Task UpsertAsync_QuandoContatoJaExiste_AtualizaNomeSemDuplicar()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var repository = new ContatoRepository(context);
        var farmaciaId = Guid.NewGuid();
        var primeiro = await repository.UpsertAsync(farmaciaId, "5511988887777", "Nome Antigo");

        // Act
        var segundo = await repository.UpsertAsync(farmaciaId, "5511988887777", "Nome Novo");

        // Assert
        Assert.Equal(primeiro.Id, segundo.Id); // mesmo contato, não duplicou
        Assert.Equal("Nome Novo", segundo.Nome);
        Assert.Single(context.Contatos); // só existe uma linha na tabela
    }

    [Fact]
    public async Task UpsertAsync_QuandoNomeNaoInformado_MantemNomeExistente()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var repository = new ContatoRepository(context);
        var farmaciaId = Guid.NewGuid();
        await repository.UpsertAsync(farmaciaId, "5511988887777", "Nome Original");

        // Act — upsert sem nome (null), simula uma interação sem pushName
        var resultado = await repository.UpsertAsync(farmaciaId, "5511988887777", null);

        // Assert
        Assert.Equal("Nome Original", resultado.Nome);
    }

    [Fact]
    public async Task AtualizarAposVendaAsync_SomaValorAoInvesDeSobrescrever()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var repository = new ContatoRepository(context);
        var farmaciaId = Guid.NewGuid();
        var contato = await repository.UpsertAsync(farmaciaId, "5511988887777", "Cliente");
        await repository.AtualizarAposVendaAsync(contato.Id, farmaciaId, DateTime.UtcNow, 50m);

        // Act — segunda compra
        var resultado = await repository.AtualizarAposVendaAsync(contato.Id, farmaciaId, DateTime.UtcNow, 30m);

        // Assert
        Assert.Equal(80m, resultado!.TotalGasto); // 50 + 30, nunca sobrescrito
    }

    [Fact]
    public async Task AtualizarAposVendaAsync_QuandoContatoEstaInativo_ReativaAutomaticamente()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var repository = new ContatoRepository(context);
        var farmaciaId = Guid.NewGuid();
        var contato = await repository.UpsertAsync(farmaciaId, "5511988887777", "Cliente");
        contato.Status = StatusContato.Inativo;
        await context.SaveChangesAsync();

        // Act
        var resultado = await repository.AtualizarAposVendaAsync(contato.Id, farmaciaId, DateTime.UtcNow, 10m);

        // Assert
        Assert.Equal(StatusContato.Ativo, resultado!.Status);
    }

    [Fact]
    public async Task AtualizarAposVendaAsync_QuandoContatoDeOutraFarmacia_RetornaNull()
    {
        // Arrange — garante isolamento entre tenants
        using var context = TestDbContextFactory.Create();
        var repository = new ContatoRepository(context);
        var farmaciaA = Guid.NewGuid();
        var farmaciaB = Guid.NewGuid();
        var contato = await repository.UpsertAsync(farmaciaA, "5511988887777", "Cliente");

        // Act — tenta atualizar usando o farmaciaId errado
        var resultado = await repository.AtualizarAposVendaAsync(contato.Id, farmaciaB, DateTime.UtcNow, 10m);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task GetInativosAsync_RetornaApenasContatosInativosDaFarmaciaCorreta()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var repository = new ContatoRepository(context);
        var farmaciaA = Guid.NewGuid();
        var farmaciaB = Guid.NewGuid();

        var ativoA = await repository.UpsertAsync(farmaciaA, "5511111111111", "Ativo A");

        var inativoA = await repository.UpsertAsync(farmaciaA, "5511222222222", "Inativo A");
        inativoA.Status = StatusContato.Inativo;

        var inativoB = await repository.UpsertAsync(farmaciaB, "5511333333333", "Inativo B (outra farmácia)");
        inativoB.Status = StatusContato.Inativo;

        await context.SaveChangesAsync();

        // Act
        var resultado = await repository.GetInativosAsync(farmaciaA);

        // Assert
        Assert.Single(resultado);
        Assert.Equal("Inativo A", resultado[0].Nome);
    }
}
