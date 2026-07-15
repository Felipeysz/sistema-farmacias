using MediatR;
using SistemaFarmacias.Application.Dtos;
using SistemaFarmacias.Application.Events;
using SistemaFarmacias.Application.Interfaces;

namespace SistemaFarmacias.Application.EventHandlers;

public class OnVendaRegistradaHandler : INotificationHandler<VendaRegistradaEvent>
{
    private readonly IContatoRepository _contatoRepository;
    private readonly IN8nWebhookNotifier _n8nWebhookNotifier;
    private readonly IVendaProcessadaRepository _vendaProcessadaRepository;

    public OnVendaRegistradaHandler(
        IContatoRepository contatoRepository,
        IN8nWebhookNotifier n8nWebhookNotifier,
        IVendaProcessadaRepository vendaProcessadaRepository)
    {
        _contatoRepository = contatoRepository;
        _n8nWebhookNotifier = n8nWebhookNotifier;
        _vendaProcessadaRepository = vendaProcessadaRepository;
    }

    public async Task Handle(VendaRegistradaEvent notification, CancellationToken cancellationToken)
    {
        // Idempotência: se essa venda (pelo VendaId) já foi processada antes
        // — reenvio por retry do PDV, ou requisição concorrente duplicada —
        // não reaplica o efeito no contato nem notifica o n8n de novo.
        var primeiraVez = await _vendaProcessadaRepository.TryRegistrarAsync(notification.VendaId);
        if (!primeiraVez)
        {
            return;
        }

        // Garante que o contato existe (cria se for a primeira compra desse telefone)
        var contato = await _contatoRepository.UpsertAsync(
            notification.FarmaciaId,
            notification.Telefone,
            notification.Nome);

        // Atualiza total gasto, última compra, e reativa se estava inativo.
        // ContatoRepository.AtualizarAposVendaAsync usa um UPDATE atômico no
        // banco (não lê-modifica-grava em memória), então isso é seguro mesmo
        // com vendas diferentes do mesmo contato chegando em paralelo.
        await _contatoRepository.AtualizarAposVendaAsync(
            contato.Id,
            notification.FarmaciaId,
            notification.RealizadaEm,
            notification.ValorTotal);

        // Notifica o n8n de forma assíncrona (via fila) — não bloqueia nem falha
        // o registro da venda se o n8n estiver fora do ar.
        var payload = new N8nVendaWebhookPayload
        {
            FarmaciaId = notification.FarmaciaId,
            ContatoId = contato.Id,
            Telefone = notification.Telefone,
            Nome = notification.Nome ?? contato.Nome,
            ValorTotal = notification.ValorTotal,
            RealizadaEm = notification.RealizadaEm,
            Produtos = notification.Produtos
        };

        await _n8nWebhookNotifier.NotificarVendaRegistradaAsync(payload);
    }
}
