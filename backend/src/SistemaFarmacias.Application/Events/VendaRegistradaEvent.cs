using MediatR;
using SistemaFarmacias.Application.Dtos;

namespace SistemaFarmacias.Application.Events;

/// <summary>
/// Evento de domínio publicado quando uma venda é registrada (hoje via endpoint mock,
/// futuramente pelo sistema de PDV real). Handlers reagem a ele sem acoplar
/// o controller à lógica de negócio (atualização de contato, notificação n8n, etc).
/// </summary>
public class VendaRegistradaEvent : INotification
{
    public Guid VendaId { get; }
    public Guid FarmaciaId { get; }
    public string Telefone { get; }
    public string? Nome { get; }
    public decimal ValorTotal { get; }
    public DateTime RealizadaEm { get; }
    public List<ItemVendaDto> Produtos { get; }

    public VendaRegistradaEvent(
        Guid vendaId,
        Guid farmaciaId,
        string telefone,
        string? nome,
        decimal valorTotal,
        DateTime realizadaEm,
        List<ItemVendaDto> produtos)
    {
        VendaId = vendaId;
        FarmaciaId = farmaciaId;
        Telefone = telefone;
        Nome = nome;
        ValorTotal = valorTotal;
        RealizadaEm = realizadaEm;
        Produtos = produtos;
    }
}
