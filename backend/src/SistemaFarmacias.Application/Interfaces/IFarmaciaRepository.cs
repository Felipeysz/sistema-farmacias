using SistemaFarmacias.Domain.Entities;

namespace SistemaFarmacias.Application.Interfaces;

public interface IFarmaciaRepository
{
    Task<WhatsappConfig?> GetByWhatsappNumberIdAsync(string whatsappNumberId);

    /// <summary>
    /// Busca a config de WhatsApp a partir do FarmaciaId — inverso de
    /// GetByWhatsappNumberIdAsync. Usado quando já se sabe qual farmácia
    /// (ex: a partir de um evento interno) e se precisa do instance id
    /// para enviar mensagens via Evolution API.
    /// </summary>
    Task<WhatsappConfig?> GetByFarmaciaIdAsync(Guid farmaciaId);
}
