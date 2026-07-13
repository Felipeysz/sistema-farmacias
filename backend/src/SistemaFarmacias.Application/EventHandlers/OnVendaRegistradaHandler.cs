using MediatR;
using SistemaFarmacias.Application.Dtos;
using SistemaFarmacias.Application.Events;
using SistemaFarmacias.Application.Interfaces;

namespace SistemaFarmacias.Application.EventHandlers;

public class OnVendaRegistradaHandler : INotificationHandler<VendaRegistradaEvent>
{
    private readonly IContatoRepository _contatoRepository;
    private readonly IN8nWebhookNotifier _n8nWebhookNotifier;

    public OnVendaRegistradaHandler(
        IContatoRepository contatoRepository,
        IN8nWebhookNotifier n8nWebhookNotifier)
    {
        _contatoRepository = contatoRepository;
        _n8nWebhookNotifier = n8nWebhookNotifier;
    }

    public async Task Handle(VendaRegistradaEvent notification, CancellationToken cancellationToken)
    {
        // Garante que o contato existe (cria se for a primeira compra desse telefone)
        var contato = await _contatoRepository.UpsertAsync(
            notification.FarmaciaId,
            notification.Telefone,
            notification.Nome);

        // Atualiza total gasto, última compra, e reativa se estava inativo
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