using System.ComponentModel.DataAnnotations;

namespace SistemaFarmacias.Application.Dtos;

public class ReativacaoCreateRequestDto
{
    [Required]
    public Guid FarmaciaId { get; set; }

    [Required]
    public Guid ContatoId { get; set; }
}
