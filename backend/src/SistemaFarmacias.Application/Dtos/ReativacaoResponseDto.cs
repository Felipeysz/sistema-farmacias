namespace SistemaFarmacias.Application.Dtos;

public class ReativacaoResponseDto
{
    public Guid Id { get; set; }
    public Guid FarmaciaId { get; set; }
    public Guid ContatoId { get; set; }
    public DateTime EnviadoEm { get; set; }
}
