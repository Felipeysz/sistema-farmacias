using SistemaFarmacias.Infrastructure.Repositories;
using SistemaFarmacias.Tests.TestHelpers;

namespace SistemaFarmacias.Tests.Repositories;

public class InteracaoRepositoryTests
{
    [Fact]
    public async Task CreateAsync_QuandoContatoExiste_CriaInteracaoESincronizaUltimaInteracaoEm()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var contatoRepository = new ContatoRepository(context);
        var interacaoRepository = new InteracaoRepository(context);
        var farmaciaId = Guid.NewGuid();
        var contato = await contatoRepository.UpsertAsync(farmaciaId, "5511988887777", "Cliente");

        Assert.Null(contato.UltimaInteracaoEm); // confirma estado inicial

        // Act
        var interacao = await interacaoRepository.CreateAsync(
            farmaciaId, contato.Id, "Oi", "Olá! Como posso ajudar?", "saudacao");

        // Assert
        Assert.NotNull(interacao);
        Assert.Equal("Oi", interacao!.MensagemRecebida);
        Assert.Equal("saudacao", interacao.IntencaoDetectada);

        var contatoAtualizado = await contatoRepository.GetByFarmaciaETelefoneAsync(farmaciaId, "5511988887777");
        Assert.NotNull(contatoAtualizado!.UltimaInteracaoEm);
    }

    [Fact]
    public async Task CreateAsync_QuandoContatoNaoPertenceAFarmacia_RetornaNull()
    {
        // Arrange — garante isolamento entre tenants
        using var context = TestDbContextFactory.Create();
        var contatoRepository = new ContatoRepository(context);
        var interacaoRepository = new InteracaoRepository(context);
        var farmaciaA = Guid.NewGuid();
        var farmaciaB = Guid.NewGuid();
        var contato = await contatoRepository.UpsertAsync(farmaciaA, "5511988887777", "Cliente");

        // Act — tenta criar interação usando o farmaciaId errado
        var interacao = await interacaoRepository.CreateAsync(
            farmaciaB, contato.Id, "Oi", null, null);

        // Assert
        Assert.Null(interacao);
    }
}
