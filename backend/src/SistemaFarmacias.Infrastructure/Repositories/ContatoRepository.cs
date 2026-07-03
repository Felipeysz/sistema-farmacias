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
            // Só sobrescreve o nome se um novo valor foi enviado,
            // para não apagar um nome já conhecido com um upsert vazio.
            contato.Nome = nome;
        }

        await _context.SaveChangesAsync();

        return contato;
    }
}
