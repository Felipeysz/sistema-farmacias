namespace SistemaFarmacias.Application.Dtos;

public class FarmaciaListItemResponseDto
{
    public Guid Id { get; set; }
    public string WhatsappNumberId { get; set; } = string.Empty;
    public string NomeExibicao { get; set; } = string.Empty;
}
