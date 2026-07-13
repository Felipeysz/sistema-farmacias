using System.ComponentModel.DataAnnotations;

namespace SistemaFarmacias.Application.Dtos;

/// <summary>
/// Payload de entrada do endpoint mock POST /api/vendas/registrar.
/// Representa o que um sistema de PDV real deveria enviar ao registrar uma venda.
/// Ver README em docs/integracao-pdv.md para o contrato completo de integração.
/// </summary>
public class VendaRegistradaRequestDto
{
    [Required]
    public Guid FarmaciaId { get; set; }

    [Required]
    public string Telefone { get; set; } = string.Empty;

    public string? Nome { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "valorTotal deve ser maior que zero.")]
    public decimal ValorTotal { get; set; }

    public DateTime RealizadaEm { get; set; } = DateTime.UtcNow;

    public List<ItemVendaDto> Produtos { get; set; } = new();
}

public class ItemVendaDto
{
    public string NomeProduto { get; set; } = string.Empty;
    public int Quantidade { get; set; } = 1;
}