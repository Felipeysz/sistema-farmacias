namespace SistemaFarmacias.Application.Dtos;

/// <summary>
/// Payload enviado à fila/webhook do n8n (Fluxo 2.1). Formato estável — o n8n
/// depende desse contrato, então mudanças aqui exigem atualizar o workflow também.
/// </summary>
public class N8nVendaWebhookPayload
{
    public Guid FarmaciaId { get; set; }
    public Guid ContatoId { get; set; }
    public string Telefone { get; set; } = string.Empty;
    public string? Nome { get; set; }
    public decimal ValorTotal { get; set; }
    public DateTime RealizadaEm { get; set; }
    public List<ItemVendaDto> Produtos { get; set; } = new();

    /// <summary>Controlado internamente pelo consumer para o mecanismo de retry.</summary>
    public int TentativaAtual { get; set; } = 1;
}