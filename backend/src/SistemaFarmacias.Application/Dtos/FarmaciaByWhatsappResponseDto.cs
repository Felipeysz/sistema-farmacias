namespace SistemaFarmacias.Application.Dtos;

public class FarmaciaByWhatsappResponseDto
{
    public Guid Id { get; set; }
    public string NomeExibicao { get; set; } = string.Empty;
    public string? HorarioFuncionamento { get; set; }
    public string? Endereco { get; set; }
    public string? MensagemSaudacao { get; set; }
}