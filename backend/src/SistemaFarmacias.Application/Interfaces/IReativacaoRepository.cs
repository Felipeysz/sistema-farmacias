using SistemaFarmacias.Domain.Entities;

namespace SistemaFarmacias.Application.Interfaces;

public interface IReativacaoRepository
{
    /// <summary>
    /// Registra que uma mensagem de reativação foi enviada a um contato.
    /// Retorna null se o contato não existir ou não pertencer à farmácia informada.
    /// </summary>
    Task<ReativacaoEnviada?> CreateAsync(Guid farmaciaId, Guid contatoId);
}
