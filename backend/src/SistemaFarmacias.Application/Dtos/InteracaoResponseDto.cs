namespace SistemaFarmacias.Application.Dtos;

public class InteracaoResponseDto
{
    public Guid Id { get; set; }
    public Guid FarmaciaId { get; set; }
    public Guid ContatoId { get; set; }
    public string Canal { get; set; } = string.Empty;
    public string? MensagemRecebida { get; set; }
    public string? MensagemEnviada { get; set; }
    public string? IntencaoDetectada { get; set; }
    public DateTime CriadoEm { get; set; }
}
