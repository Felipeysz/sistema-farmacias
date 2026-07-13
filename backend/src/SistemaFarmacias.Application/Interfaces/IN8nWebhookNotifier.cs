using SistemaFarmacias.Application.Dtos;

namespace SistemaFarmacias.Application.Interfaces;

/// <summary>
/// Publica notificações para o n8n de forma assíncrona (via fila), para que
/// uma falha ou indisponibilidade do n8n não bloqueie o fluxo principal
/// de registro de venda.
/// </summary>
public interface IN8nWebhookNotifier
{
    Task NotificarVendaRegistradaAsync(N8nVendaWebhookPayload payload);
}