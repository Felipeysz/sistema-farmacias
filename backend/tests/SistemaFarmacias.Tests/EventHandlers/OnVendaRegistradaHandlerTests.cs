using Moq;
using SistemaFarmacias.Application.Dtos;
using SistemaFarmacias.Application.EventHandlers;
using SistemaFarmacias.Application.Events;
using SistemaFarmacias.Application.Interfaces;
using SistemaFarmacias.Domain.Entities;

namespace SistemaFarmacias.Tests.EventHandlers;

public class OnVendaRegistradaHandlerTests
{
    private static VendaRegistradaEvent CriarEvento(
        Guid? vendaId = null,
        Guid? farmaciaId = null,
        string telefone = "5511988887777",
        string? nome = "Cliente Teste",
        decimal valorTotal = 49.90m) =>
        new(
            vendaId ?? Guid.NewGuid(),
            farmaciaId ?? Guid.NewGuid(),
            telefone,
            nome,
            valorTotal,
            DateTime.UtcNow,
            new List<ItemVendaDto> { new() { NomeProduto = "Dipirona", Quantidade = 2 } });

    /// <summary>
    /// Mock de IVendaProcessadaRepository que sempre reporta "primeira vez"
    /// (não duplicata) — usado nos testes que focam no fluxo de negócio em
    /// si, não na idempotência (essa é coberta pelos testes de integração).
    /// </summary>
    private static Mock<IVendaProcessadaRepository> CriarVendaProcessadaRepositoryMock()
    {
        var mock = new Mock<IVendaProcessadaRepository>();
        mock.Setup(r => r.TryRegistrarAsync(It.IsAny<Guid>())).ReturnsAsync(true);
        return mock;
    }

    [Fact]
    public async Task Handle_FluxoFeliz_ChamaUpsertAtualizaVendaENotificaN8n()
    {
        // Arrange
        var farmaciaId = Guid.NewGuid();
        var contatoId = Guid.NewGuid();
        var evento = CriarEvento(farmaciaId: farmaciaId);

        var contatoMock = new Contato
        {
            Id = contatoId,
            FarmaciaId = farmaciaId,
            Telefone = evento.Telefone,
            Nome = evento.Nome
        };

        var contatoRepository = new Mock<IContatoRepository>();
        contatoRepository
            .Setup(r => r.UpsertAsync(farmaciaId, evento.Telefone, evento.Nome))
            .ReturnsAsync(contatoMock);
        contatoRepository
            .Setup(r => r.AtualizarAposVendaAsync(contatoId, farmaciaId, evento.RealizadaEm, evento.ValorTotal))
            .ReturnsAsync(contatoMock);

        var notifier = new Mock<IN8nWebhookNotifier>();
        var vendaProcessadaRepository = CriarVendaProcessadaRepositoryMock();

        var handler = new OnVendaRegistradaHandler(
            contatoRepository.Object, notifier.Object, vendaProcessadaRepository.Object);

        // Act
        await handler.Handle(evento, CancellationToken.None);

        // Assert — upsert e atualização de venda foram chamados com os dados corretos
        contatoRepository.Verify(
            r => r.UpsertAsync(farmaciaId, evento.Telefone, evento.Nome),
            Times.Once);
        contatoRepository.Verify(
            r => r.AtualizarAposVendaAsync(contatoId, farmaciaId, evento.RealizadaEm, evento.ValorTotal),
            Times.Once);

        // Assert — notificou o n8n com o payload correto
        notifier.Verify(
            n => n.NotificarVendaRegistradaAsync(It.Is<N8nVendaWebhookPayload>(p =>
                p.FarmaciaId == farmaciaId &&
                p.ContatoId == contatoId &&
                p.Telefone == evento.Telefone &&
                p.ValorTotal == evento.ValorTotal &&
                p.Produtos.Count == 1)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_QuandoVendaJaFoiProcessada_NaoChamaNadaMais()
    {
        // Arrange — cobre a idempotência no nível do handler (unitário,
        // complementar aos testes de integração que testam concorrência real)
        var evento = CriarEvento();

        var contatoRepository = new Mock<IContatoRepository>();
        var notifier = new Mock<IN8nWebhookNotifier>();
        var vendaProcessadaRepository = new Mock<IVendaProcessadaRepository>();
        vendaProcessadaRepository
            .Setup(r => r.TryRegistrarAsync(evento.VendaId))
            .ReturnsAsync(false); // já processada antes

        var handler = new OnVendaRegistradaHandler(
            contatoRepository.Object, notifier.Object, vendaProcessadaRepository.Object);

        // Act
        await handler.Handle(evento, CancellationToken.None);

        // Assert — nada do fluxo de negócio foi chamado
        contatoRepository.Verify(
            r => r.UpsertAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>()),
            Times.Never);
        contatoRepository.Verify(
            r => r.AtualizarAposVendaAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<decimal>()),
            Times.Never);
        notifier.Verify(
            n => n.NotificarVendaRegistradaAsync(It.IsAny<N8nVendaWebhookPayload>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_QuandoNomeDoEventoNaoInformado_UsaNomeDoContatoExistenteNoPayload()
    {
        // Arrange — simula uma venda de um cliente já cadastrado, sem nome no request
        var farmaciaId = Guid.NewGuid();
        var contatoId = Guid.NewGuid();
        var evento = CriarEvento(farmaciaId: farmaciaId, nome: null);

        var contatoExistente = new Contato
        {
            Id = contatoId,
            FarmaciaId = farmaciaId,
            Telefone = evento.Telefone,
            Nome = "Nome Já Cadastrado"
        };

        var contatoRepository = new Mock<IContatoRepository>();
        contatoRepository
            .Setup(r => r.UpsertAsync(farmaciaId, evento.Telefone, null))
            .ReturnsAsync(contatoExistente);
        contatoRepository
            .Setup(r => r.AtualizarAposVendaAsync(contatoId, farmaciaId, evento.RealizadaEm, evento.ValorTotal))
            .ReturnsAsync(contatoExistente);

        var notifier = new Mock<IN8nWebhookNotifier>();
        var vendaProcessadaRepository = CriarVendaProcessadaRepositoryMock();
        var handler = new OnVendaRegistradaHandler(
            contatoRepository.Object, notifier.Object, vendaProcessadaRepository.Object);

        // Act
        await handler.Handle(evento, CancellationToken.None);

        // Assert — payload usa o nome do contato (fallback), não null
        notifier.Verify(
            n => n.NotificarVendaRegistradaAsync(It.Is<N8nVendaWebhookPayload>(p =>
                p.Nome == "Nome Já Cadastrado")),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ChamaOperacoesNaOrdemCorreta_UpsertAntesDeAtualizarVenda()
    {
        // Arrange — garante que o contato é garantido (upsert) antes de tentar
        // atualizar a venda nele, já que AtualizarAposVendaAsync depende do Id gerado
        var farmaciaId = Guid.NewGuid();
        var contatoId = Guid.NewGuid();
        var evento = CriarEvento(farmaciaId: farmaciaId);
        var contatoMock = new Contato { Id = contatoId, FarmaciaId = farmaciaId, Telefone = evento.Telefone };

        var chamadas = new List<string>();

        var contatoRepository = new Mock<IContatoRepository>();
        contatoRepository
            .Setup(r => r.UpsertAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>()))
            .Callback(() => chamadas.Add("upsert"))
            .ReturnsAsync(contatoMock);
        contatoRepository
            .Setup(r => r.AtualizarAposVendaAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<decimal>()))
            .Callback(() => chamadas.Add("atualizar_venda"))
            .ReturnsAsync(contatoMock);

        var notifier = new Mock<IN8nWebhookNotifier>();
        var vendaProcessadaRepository = CriarVendaProcessadaRepositoryMock();
        var handler = new OnVendaRegistradaHandler(
            contatoRepository.Object, notifier.Object, vendaProcessadaRepository.Object);

        // Act
        await handler.Handle(evento, CancellationToken.None);

        // Assert
        Assert.Equal(new[] { "upsert", "atualizar_venda" }, chamadas);
    }
}
