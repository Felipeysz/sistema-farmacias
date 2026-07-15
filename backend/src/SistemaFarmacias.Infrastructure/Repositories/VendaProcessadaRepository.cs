using Microsoft.EntityFrameworkCore;
using SistemaFarmacias.Application.Interfaces;
using SistemaFarmacias.Domain.Entities;
using SistemaFarmacias.Infrastructure.Persistence;

namespace SistemaFarmacias.Infrastructure.Repositories;

public class VendaProcessadaRepository : IVendaProcessadaRepository
{
    private readonly AppDbContext _context;

    public VendaProcessadaRepository(AppDbContext context) => _context = context;

    public async Task<bool> TryRegistrarAsync(Guid vendaId)
    {
        _context.VendasProcessadas.Add(new VendaProcessada { VendaId = vendaId });

        try
        {
            await _context.SaveChangesAsync();
            return true; // primeira vez — pode processar a venda
        }
        catch (DbUpdateException)
        {
            // Violação da PK: essa venda já foi (ou está sendo, por outra
            // requisição concorrente) processada. Não é um erro real —
            // é exatamente o comportamento de idempotência esperado.
            _context.Entry(_context.VendasProcessadas.Local.First(v => v.VendaId == vendaId)).State = EntityState.Detached;
            return false;
        }
    }
}
