using System.ComponentModel.DataAnnotations;

namespace SistemaFarmacias.Application.Dtos;

/// <summary>
/// Payload de entrada do endpoint mock POST /api/vendas/registrar.
/// Representa o que um sistema de PDV real deveria enviar ao registrar uma venda.
/// Ver README em docs/integracao-pdv.md para o contrato completo de integração.
/// </summary>
public class VendaRegistradaRequestDto
{
    /// <summary>
    /// Identificador único da venda, gerado por quem chama (o PDV). É a chave
    /// de idempotência: reenviar a mesma venda com o mesmo VendaId (ex: retry
    /// após timeout de rede) não duplica o efeito no contato.
    /// </summary>
    [Required]
    public Guid VendaId { get; set; }

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
