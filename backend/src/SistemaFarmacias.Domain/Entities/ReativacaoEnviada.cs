namespace SistemaFarmacias.Domain.Entities;

public class ReativacaoEnviada
{
    public Guid Id { get; set; }
    public Guid FarmaciaId { get; set; }
    public Guid ContatoId { get; set; }
    public DateTime EnviadoEm { get; set; } = DateTime.UtcNow;

    public Farmacia Farmacia { get; set; } = null!;
    public Contato Contato { get; set; } = null!;
}