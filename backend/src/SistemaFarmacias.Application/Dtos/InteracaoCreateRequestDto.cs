using System.ComponentModel.DataAnnotations;

namespace SistemaFarmacias.Application.Dtos;

public class InteracaoCreateRequestDto
{
    [Required]
    public Guid FarmaciaId { get; set; }

    [Required]
    public Guid ContatoId { get; set; }

    public string? MensagemRecebida { get; set; }

    public string? MensagemEnviada { get; set; }

    public string? IntencaoDetectada { get; set; }
}
