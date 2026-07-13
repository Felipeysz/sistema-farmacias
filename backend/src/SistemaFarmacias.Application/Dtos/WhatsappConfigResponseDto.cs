namespace SistemaFarmacias.Application.Dtos;

/// <summary>
/// Usado quando já se tem o FarmaciaId (ex: vindo de um evento interno como
/// VendaRegistradaEvent) e se precisa descobrir o WhatsappNumberId (instance
/// do Evolution API) para enviar mensagens — o inverso do fluxo by-whatsapp.
/// </summary>
public class WhatsappConfigResponseDto
{
    public Guid FarmaciaId { get; set; }
    public string WhatsappNumberId { get; set; } = string.Empty;
    public string NomeExibicao { get; set; } = string.Empty;
}
