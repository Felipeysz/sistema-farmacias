using System.ComponentModel.DataAnnotations;

namespace SistemaFarmacias.Application.Dtos;

public class ContatoAtualizarAposVendaRequestDto
{
    [Required]
    public Guid FarmaciaId { get; set; }

    [Required]
    public DateTime DataUltimaCompra { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "valorCompra deve ser maior que zero.")]
    public decimal ValorCompra { get; set; }
}
