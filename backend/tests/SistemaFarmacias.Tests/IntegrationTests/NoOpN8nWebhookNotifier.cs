using SistemaFarmacias.Application.Dtos;
using SistemaFarmacias.Application.Interfaces;

namespace SistemaFarmacias.Tests.IntegrationTests;

/// <summary>
/// Substitui o N8nWebhookNotifier real nos testes de integração — não existe
/// RabbitMQ disponível nesse ambiente (só o Postgres é provisionado via
/// Testcontainers), e o notifier real lançaria exceção de conexão, quebrando
/// qualquer teste que exercite o fluxo de vendas com um 500 não relacionado
/// ao que está sendo testado (idempotência/concorrência do contato).
/// </summary>
public class NoOpN8nWebhookNotifier : IN8nWebhookNotifier
{
    public Task NotificarVendaRegistradaAsync(N8nVendaWebhookPayload payload) => Task.CompletedTask;
}
