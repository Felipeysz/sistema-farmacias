using Microsoft.EntityFrameworkCore;
using SistemaFarmacias.Application.Interfaces;
using SistemaFarmacias.Domain.Entities;
using SistemaFarmacias.Infrastructure.Persistence;

namespace SistemaFarmacias.Infrastructure.Repositories;

public class ContatoRepository : IContatoRepository
{
    private readonly AppDbContext _context;

    public ContatoRepository(AppDbContext context) => _context = context;

    public Task<Contato?> GetByFarmaciaETelefoneAsync(Guid farmaciaId, string telefone)
    {
        return _context.Contatos
            .FirstOrDefaultAsync(c => c.FarmaciaId == farmaciaId && c.Telefone == telefone);
    }

    public async Task<Contato> UpsertAsync(Guid farmaciaId, string telefone, string? nome)
    {
        var contato = await GetByFarmaciaETelefoneAsync(farmaciaId, telefone);

        if (contato is null)
        {
            contato = new Contato
            {
                Id = Guid.NewGuid(),
                FarmaciaId = farmaciaId,
                Telefone = telefone,
                Nome = nome,
                Status = StatusContato.Ativo
            };

            _context.Contatos.Add(contato);
        }
        else if (!string.IsNullOrWhiteSpace(nome))
        {
            contato.Nome = nome;
        }

        await _context.SaveChangesAsync();

        return contato;
    }

    public async Task<Contato?> AtualizarAposVendaAsync(
        Guid contatoId,
        Guid farmaciaId,
        DateTime dataUltimaCompra,
        decimal valorCompra)
    {
        var contato = await _context.Contatos
            .FirstOrDefaultAsync(c => c.Id == contatoId && c.FarmaciaId == farmaciaId);

        if (contato is null)
            return null;

        contato.TotalGasto += valorCompra;
        contato.UltimaCompraEm = dataUltimaCompra;

        if (contato.Status == StatusContato.Inativo)
            contato.Status = StatusContato.Ativo;

        await _context.SaveChangesAsync();

        return contato;
    }

    public Task<List<Contato>> GetInativosAsync(Guid farmaciaId)
    {
        return _context.Contatos
            .Where(c => c.FarmaciaId == farmaciaId && c.Status == StatusContato.Inativo)
            .OrderBy(c => c.UltimaCompraEm)
            .ToListAsync();
    }
}
