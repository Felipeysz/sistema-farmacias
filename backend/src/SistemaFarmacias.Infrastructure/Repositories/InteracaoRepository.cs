using Microsoft.EntityFrameworkCore;
using SistemaFarmacias.Application.Interfaces;
using SistemaFarmacias.Domain.Entities;
using SistemaFarmacias.Infrastructure.Persistence;

namespace SistemaFarmacias.Infrastructure.Repositories;

public class InteracaoRepository : IInteracaoRepository
{
    private readonly AppDbContext _context;

    public InteracaoRepository(AppDbContext context) => _context = context;

    public async Task<Interacao?> CreateAsync(
        Guid farmaciaId,
        Guid contatoId,
        string? mensagemRecebida,
        string? mensagemEnviada,
        string? intencaoDetectada)
    {
        var contato = await _context.Contatos
            .FirstOrDefaultAsync(c => c.Id == contatoId && c.FarmaciaId == farmaciaId);

        if (contato is null)
            return null;

        var interacao = new Interacao
        {
            Id = Guid.NewGuid(),
            FarmaciaId = farmaciaId,
            ContatoId = contatoId,
            Canal = CanalInteracao.WhatsApp,
            MensagemRecebida = mensagemRecebida,
            MensagemEnviada = mensagemEnviada,
            IntencaoDetectada = intencaoDetectada,
            CriadoEm = DateTime.UtcNow
        };

        _context.Interacoes.Add(interacao);

        // Mantém o contato sincronizado com a interação mais recente
        contato.UltimaInteracaoEm = interacao.CriadoEm;

        await _context.SaveChangesAsync();

        return interacao;
    }
}
