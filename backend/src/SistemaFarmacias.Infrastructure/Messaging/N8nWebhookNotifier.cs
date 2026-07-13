using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using SistemaFarmacias.Application.Dtos;
using SistemaFarmacias.Application.Interfaces;

namespace SistemaFarmacias.Infrastructure.Messaging;

/// <summary>
/// Publica a notificação de venda registrada numa fila durável do RabbitMQ,
/// em vez de chamar o webhook do n8n diretamente. Isso desacopla o registro
/// da venda de eventuais indisponibilidades do n8n — o N8nWebhookConsumerService
/// consome a fila e faz o envio de fato, com retry.
/// </summary>
public class N8nWebhookNotifier : IN8nWebhookNotifier
{
    private readonly RabbitMqSettings _settings;

    public N8nWebhookNotifier(RabbitMqSettings settings)
    {
        _settings = settings;
    }

    public async Task NotificarVendaRegistradaAsync(N8nVendaWebhookPayload payload)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            VirtualHost = _settings.VirtualHost,
            UserName = _settings.UserName,
            Password = _settings.Password
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: RabbitMqSettings.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        var json = JsonSerializer.Serialize(payload);
        var body = Encoding.UTF8.GetBytes(json);

        var props = new BasicProperties { Persistent = true };

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: RabbitMqSettings.QueueName,
            mandatory: false,
            basicProperties: props,
            body: body);
    }
}