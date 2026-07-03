namespace SistemaFarmacias.Application.Dtos;

public class ContatoResponseDto
{
    public Guid Id { get; set; }
    public Guid FarmaciaId { get; set; }
    public string Telefone { get; set; } = string.Empty;
    public string? Nome { get; set; }
    public DateTime? UltimaInteracaoEm { get; set; }
    public DateTime? UltimaCompraEm { get; set; }
    public decimal TotalGasto { get; set; }
    public string Status { get; set; } = string.Empty;
}
