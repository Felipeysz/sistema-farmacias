using Microsoft.EntityFrameworkCore;
using SistemaFarmacias.Application.Interfaces;
using SistemaFarmacias.Domain.Entities;
using SistemaFarmacias.Infrastructure.Persistence;

namespace SistemaFarmacias.Infrastructure.Repositories;

public class FarmaciaRepository : IFarmaciaRepository
{
    private readonly AppDbContext _context;

    public FarmaciaRepository(AppDbContext context) => _context = context;

    public Task<WhatsappConfig?> GetByWhatsappNumberIdAsync(string whatsappNumberId)
    {
        return _context.WhatsappConfigs
            .Include(w => w.Farmacia)
            .FirstOrDefaultAsync(w => w.WhatsappNumberId == whatsappNumberId);
    }

    public Task<WhatsappConfig?> GetByFarmaciaIdAsync(Guid farmaciaId)
    {
        return _context.WhatsappConfigs
            .Include(w => w.Farmacia)
            .FirstOrDefaultAsync(w => w.FarmaciaId == farmaciaId);
    }

    public Task<List<WhatsappConfig>> GetAllAsync()
    {
        return _context.WhatsappConfigs
            .Include(w => w.Farmacia)
            .ToListAsync();
    }
}
