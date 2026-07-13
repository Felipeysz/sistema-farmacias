namespace SistemaFarmacias.Infrastructure.Messaging;

public class RabbitMqSettings
{
    public string HostName { get; set; } = "rabbitmq";
    public int Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public const string QueueName = "n8n_venda_registrada";
}