using SistemaFarmacias.Domain.Entities;

namespace SistemaFarmacias.Application.Interfaces;

public interface IContatoRepository
{
    Task<Contato?> GetByFarmaciaETelefoneAsync(Guid farmaciaId, string telefone);
    Task<Contato> UpsertAsync(Guid farmaciaId, string telefone, string? nome);

    Task<Contato?> AtualizarAposVendaAsync(Guid contatoId, Guid farmaciaId, DateTime dataUltimaCompra, decimal valorCompra);

    /// <summary>
    /// Lista os contatos com Status = Inativo de uma farmácia específica.
    /// Usado pelo Fluxo 2.2 (schedule diário de reativação).
    /// </summary>
    Task<List<Contato>> GetInativosAsync(Guid farmaciaId);
}
