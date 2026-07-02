namespace SistemaFarmacias.Domain.Entities;

public enum StatusContato
{
    Ativo,
    Inativo
}

public class Contato
{
    public Guid Id { get; set; }
    public Guid FarmaciaId { get; set; }
    public string Telefone { get; set; } = string.Empty;
    public string? Nome { get; set; }
    public DateTime? UltimaInteracaoEm { get; set; }
    public DateTime? UltimaCompraEm { get; set; }
    public decimal TotalGasto { get; set; } = 0;
    public StatusContato Status { get; set; } = StatusContato.Ativo;

    public Farmacia Farmacia { get; set; } = null!;
    public ICollection<Interacao> Interacoes { get; set; } = new List<Interacao>();
    public ICollection<ReativacaoEnviada> Reativacoes { get; set; } = new List<ReativacaoEnviada>();
}