namespace SistemaFarmacias.Application.Interfaces;

public interface IVendaProcessadaRepository
{
    /// <summary>
    /// Tenta registrar a venda como processada. Retorna true se essa foi a
    /// primeira vez (deve processar o efeito da venda), ou false se a venda
    /// já tinha sido processada antes (deve ser ignorada — idempotência).
    ///
    /// Seguro sob concorrência real: duas chamadas simultâneas com o mesmo
    /// vendaId nunca retornam true nas duas — a PK do Postgres garante isso.
    /// </summary>
    Task<bool> TryRegistrarAsync(Guid vendaId);
}
