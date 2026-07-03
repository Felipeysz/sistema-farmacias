using SistemaFarmacias.Domain.Entities;

namespace SistemaFarmacias.Application.Interfaces;

public interface IInteracaoRepository
{
    /// <summary>
    /// Cria uma nova interação e atualiza o campo UltimaInteracaoEm do contato.
    /// Retorna null se o contato informado não existir.
    /// </summary>
    Task<Interacao?> CreateAsync(
        Guid farmaciaId,
        Guid contatoId,
        string? mensagemRecebida,
        string? mensagemEnviada,
        string? intencaoDetectada);
}
