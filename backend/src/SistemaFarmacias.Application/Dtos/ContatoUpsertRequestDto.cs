using System.ComponentModel.DataAnnotations;

namespace SistemaFarmacias.Application.Dtos;

public class ContatoUpsertRequestDto
{
    [Required]
    public Guid FarmaciaId { get; set; }

    [Required]
    public string Telefone { get; set; } = string.Empty;

    public string? Nome { get; set; }
}
