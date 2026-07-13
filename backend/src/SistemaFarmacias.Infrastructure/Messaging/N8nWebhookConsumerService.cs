using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SistemaFarmacias.Application.Dtos;

namespace SistemaFarmacias.Infrastructure.Messaging;

/// <summary>
/// Consome a fila n8n_venda_registrada e envia o webhook HTTP ao n8n (Fluxo 2.1).
/// Em caso de falha, republica a mensagem com contador de tentativa incrementado
/// e um atraso (backoff exponencial simples, em memória). Após MaxTentativas,
/// a mensagem é descartada e o erro é logado — para produção, considerar migrar
/// para um Dead Letter Exchange nativo do RabbitMQ em vez desse retry manual.
/// </summary>
public class N8nWebhookConsumerService : BackgroundService
{
    private const int MaxTentativas = 5;

    private readonly RabbitMqSettings _settings;
    private readonly string _webhookUrl;
    private readonly ILogger<N8nWebhookConsumerService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    private IConnection? _connection;
    private IChannel? _channel;

    public N8nWebhookConsumerService(
        RabbitMqSettings settings,
        string webhookUrl,
        ILogger<N8nWebhookConsumerService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _settings = settings;
        _webhookUrl = webhookUrl;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            VirtualHost = _settings.VirtualHost,
            UserName = _settings.UserName,
            Password = _settings.Password
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: RabbitMqSettings.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var payload = JsonSerializer.Deserialize<N8nVendaWebhookPayload>(json);

            if (payload is null)
            {
                await _channel.BasicAckAsync(ea.DeliveryTag, false);
                return;
            }

            var sucesso = await TentarEnviarAsync(payload, stoppingToken);

            if (sucesso)
            {
                await _channel.BasicAckAsync(ea.DeliveryTag, false);
                return;
            }

            if (payload.TentativaAtual >= MaxTentativas)
            {
                _logger.LogError(
                    "Desistindo de notificar n8n após {Tentativas} tentativas. FarmaciaId={FarmaciaId} ContatoId={ContatoId}",
                    payload.TentativaAtual, payload.FarmaciaId, payload.ContatoId);
                await _channel.BasicAckAsync(ea.DeliveryTag, false); // descarta
                return;
            }

            // Backoff exponencial simples antes de republicar (2s, 4s, 8s, 16s...)
            var atraso = TimeSpan.FromSeconds(Math.Pow(2, payload.TentativaAtual));
            await Task.Delay(atraso, stoppingToken);

            payload.TentativaAtual++;
            var novoJson = JsonSerializer.Serialize(payload);
            var novoBody = Encoding.UTF8.GetBytes(novoJson);
            var props = new BasicProperties { Persistent = true };

            await _channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: RabbitMqSettings.QueueName,
                mandatory: false,
                basicProperties: props,
                body: novoBody,
                cancellationToken: stoppingToken);

            await _channel.BasicAckAsync(ea.DeliveryTag, false);
        };

        await _channel.BasicConsumeAsync(
            queue: RabbitMqSettings.QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        // Mantém o BackgroundService vivo até o app ser encerrado
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task<bool> TentarEnviarAsync(N8nVendaWebhookPayload payload, CancellationToken ct)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(_webhookUrl, content, ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Falha ao notificar n8n (tentativa {Tentativa}). FarmaciaId={FarmaciaId}",
                payload.TentativaAtual, payload.FarmaciaId);
            return false;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null) await _channel.CloseAsync(cancellationToken);
        if (_connection is not null) await _connection.CloseAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}