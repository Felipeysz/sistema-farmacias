namespace SistemaFarmacias.Domain.Entities;

public class WhatsappConfig
{
    public Guid Id { get; set; }
    public Guid FarmaciaId { get; set; }
    public string WhatsappNumberId { get; set; } = string.Empty; // único
    public string NomeExibicao { get; set; } = string.Empty;
    public string? HorarioFuncionamento { get; set; }
    public string? Endereco { get; set; }
    public string? MensagemSaudacao { get; set; }

    public Farmacia Farmacia { get; set; } = null!;
}