using SistemaFarmacias.Domain.Entities;

namespace SistemaFarmacias.Application.Interfaces;

public interface IFarmaciaRepository
{
    Task<WhatsappConfig?> GetByWhatsappNumberIdAsync(string whatsappNumberId);
}