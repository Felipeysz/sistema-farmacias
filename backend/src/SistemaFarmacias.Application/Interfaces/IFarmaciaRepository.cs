using SistemaFarmacias.Domain.Entities;

namespace SistemaFarmacias.Application.Interfaces;

public interface IFarmaciaRepository
{
    Task<WhatsappConfig?> GetByWhatsappNumberIdAsync(string whatsappNumberId);

    Task<WhatsappConfig?> GetByFarmaciaIdAsync(Guid farmaciaId);

    /// <summary>
    /// Lista todas as farmácias com WhatsApp configurado. Usado pelo Fluxo 2.2
    /// (schedule diário de reativação), que precisa iterar sobre todas as
    /// farmácias em vez de depender de um farmaciaId fixo.
    /// </summary>
    Task<List<WhatsappConfig>> GetAllAsync();
}
