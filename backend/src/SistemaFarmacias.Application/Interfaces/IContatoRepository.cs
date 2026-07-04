using SistemaFarmacias.Domain.Entities;

namespace SistemaFarmacias.Application.Interfaces;

public interface IContatoRepository
{
    Task<Contato?> GetByFarmaciaETelefoneAsync(Guid farmaciaId, string telefone);
    Task<Contato> UpsertAsync(Guid farmaciaId, string telefone, string? nome);
    Task<Contato?> AtualizarAposVendaAsync(Guid contatoId, Guid farmaciaId, DateTime dataUltimaCompra, decimal valorCompra);
}
