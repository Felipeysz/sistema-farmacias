using Microsoft.EntityFrameworkCore;
using SistemaFarmacias.Application.Interfaces;
using SistemaFarmacias.Domain.Entities;
using SistemaFarmacias.Infrastructure.Persistence;

namespace SistemaFarmacias.Infrastructure.Repositories;

public class ReativacaoRepository : IReativacaoRepository
{
    private readonly AppDbContext _context;

    public ReativacaoRepository(AppDbContext context) => _context = context;

    public async Task<ReativacaoEnviada?> CreateAsync(Guid farmaciaId, Guid contatoId)
    {
        // Garante isolamento entre tenants: só registra a reativação se o
        // contato realmente pertencer à farmácia informada.
        var contatoExiste = await _context.Contatos
            .AnyAsync(c => c.Id == contatoId && c.FarmaciaId == farmaciaId);

        if (!contatoExiste)
            return null;

        var reativacao = new ReativacaoEnviada
        {
            Id = Guid.NewGuid(),
            FarmaciaId = farmaciaId,
            ContatoId = contatoId,
            EnviadoEm = DateTime.UtcNow
        };

        _context.ReativacoesEnviadas.Add(reativacao);
        await _context.SaveChangesAsync();

        return reativacao;
    }
}
