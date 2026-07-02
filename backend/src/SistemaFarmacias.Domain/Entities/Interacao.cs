namespace SistemaFarmacias.Domain.Entities;

public enum CanalInteracao
{
    WhatsApp
}

public class Interacao
{
    public Guid Id { get; set; }
    public Guid FarmaciaId { get; set; }
    public Guid ContatoId { get; set; }
    public CanalInteracao Canal { get; set; } = CanalInteracao.WhatsApp;
    public string? MensagemRecebida { get; set; }
    public string? MensagemEnviada { get; set; }
    public string? IntencaoDetectada { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public Farmacia Farmacia { get; set; } = null!;
    public Contato Contato { get; set; } = null!;
}