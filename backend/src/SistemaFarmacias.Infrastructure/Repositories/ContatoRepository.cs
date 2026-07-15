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

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Corrida: outra requisição concorrente criou o mesmo contato
                // (mesmo FarmaciaId+Telefone) entre o SELECT acima e este INSERT.
                // O índice único do Postgres pegou isso — busca de novo e usa
                // o que já existe, em vez de propagar o erro.
                _context.Entry(contato).State = EntityState.Detached;
                contato = await GetByFarmaciaETelefoneAsync(farmaciaId, telefone)
                    ?? throw new InvalidOperationException(
                        "Falha ao criar contato e não foi possível recuperá-lo após conflito de concorrência.");

                if (!string.IsNullOrWhiteSpace(nome))
                {
                    contato.Nome = nome;
                    await _context.SaveChangesAsync();
                }
            }
        }
        else if (!string.IsNullOrWhiteSpace(nome))
        {
            contato.Nome = nome;
            await _context.SaveChangesAsync();
        }

        return contato;
    }

    public async Task<Contato?> AtualizarAposVendaAsync(
        Guid contatoId,
        Guid farmaciaId,
        DateTime dataUltimaCompra,
        decimal valorCompra)
    {
        // ExecuteUpdateAsync gera um único UPDATE atômico no Postgres
        // (ex: SET total_gasto = total_gasto + @valor), em vez do padrão
        // "SELECT, altera em memória, SAVE" — isso elimina o lost update
        // clássico quando duas vendas do mesmo contato chegam em paralelo:
        // cada UPDATE soma sobre o valor mais recente já persistido, nunca
        // sobre um valor "desatualizado" lido em memória por outra thread.
        var linhasAfetadas = await _context.Contatos
            .Where(c => c.Id == contatoId && c.FarmaciaId == farmaciaId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.TotalGasto, c => c.TotalGasto + valorCompra)
                .SetProperty(c => c.UltimaCompraEm, dataUltimaCompra)
                .SetProperty(c => c.Status, c =>
                    c.Status == StatusContato.Inativo ? StatusContato.Ativo : c.Status));

        if (linhasAfetadas == 0)
            return null;

        return await _context.Contatos
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == contatoId && c.FarmaciaId == farmaciaId);
    }

    public Task<List<Contato>> GetInativosAsync(Guid farmaciaId)
    {
        return _context.Contatos
            .Where(c => c.FarmaciaId == farmaciaId && c.Status == StatusContato.Inativo)
            .OrderBy(c => c.UltimaCompraEm)
            .ToListAsync();
    }
}
