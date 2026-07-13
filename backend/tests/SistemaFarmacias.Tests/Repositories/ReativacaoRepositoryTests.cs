using SistemaFarmacias.Infrastructure.Repositories;
using SistemaFarmacias.Tests.TestHelpers;

namespace SistemaFarmacias.Tests.Repositories;

public class ReativacaoRepositoryTests
{
    [Fact]
    public async Task CreateAsync_QuandoContatoExiste_RegistraReativacao()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var contatoRepository = new ContatoRepository(context);
        var reativacaoRepository = new ReativacaoRepository(context);
        var farmaciaId = Guid.NewGuid();
        var contato = await contatoRepository.UpsertAsync(farmaciaId, "5511988887777", "Cliente");

        // Act
        var reativacao = await reativacaoRepository.CreateAsync(farmaciaId, contato.Id);

        // Assert
        Assert.NotNull(reativacao);
        Assert.Equal(farmaciaId, reativacao!.FarmaciaId);
        Assert.Equal(contato.Id, reativacao.ContatoId);
    }

    [Fact]
    public async Task CreateAsync_QuandoContatoNaoPertenceAFarmacia_RetornaNull()
    {
        // Arrange — garante isolamento entre tenants
        using var context = TestDbContextFactory.Create();
        var contatoRepository = new ContatoRepository(context);
        var reativacaoRepository = new ReativacaoRepository(context);
        var farmaciaA = Guid.NewGuid();
        var farmaciaB = Guid.NewGuid();
        var contato = await contatoRepository.UpsertAsync(farmaciaA, "5511988887777", "Cliente");

        // Act
        var reativacao = await reativacaoRepository.CreateAsync(farmaciaB, contato.Id);

        // Assert
        Assert.Null(reativacao);
    }

    [Fact]
    public async Task CreateAsync_QuandoContatoNaoExiste_RetornaNull()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var reativacaoRepository = new ReativacaoRepository(context);

        // Act
        var reativacao = await reativacaoRepository.CreateAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.Null(reativacao);
    }
}
