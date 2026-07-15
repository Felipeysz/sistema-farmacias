namespace SistemaFarmacias.Domain.Entities;

/// <summary>
/// Registra que uma venda (identificada por VendaId) já foi processada pelo
/// OnVendaRegistradaHandler. O VendaId é a própria chave primária — isso
/// garante, mesmo sob concorrência real (duas requisições simultâneas com o
/// mesmo VendaId), que o efeito da venda nunca é aplicado duas vezes: a
/// segunda tentativa de inserção esbarra na constraint de PK do Postgres.
/// </summary>
public class VendaProcessada
{
    public Guid VendaId { get; set; }
    public DateTime ProcessadaEm { get; set; } = DateTime.UtcNow;
}
