using MediatR;
using Microsoft.AspNetCore.Mvc;
using SistemaFarmacias.Application.Dtos;
using SistemaFarmacias.Application.Events;

namespace SistemaFarmacias.Api.Controllers;

/// <summary>
/// Endpoint MOCK que simula o registro de uma venda no PDV da farmácia.
/// Objetivo: permitir testar o Fluxo 2 (n8n) ponta a ponta antes da integração
/// real com o sistema de vendas do cliente.
///
/// CONTRATO DE INTEGRAÇÃO FUTURA: quando o sistema de PDV real estiver pronto,
/// ele deve fazer uma chamada equivalente a esta (mesmo payload) — seja
/// substituindo este endpoint, seja publicando o VendaRegistradaEvent
/// diretamente se rodar no mesmo processo. Ver docs/integracao-pdv.md.
/// </summary>
[ApiController]
[Route("api/vendas")]
public class VendasController : ControllerBase
{
    private readonly IMediator _mediator;

    public VendasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar([FromBody] VendaRegistradaRequestDto request)
    {
        var evento = new VendaRegistradaEvent(
            request.FarmaciaId,
            request.Telefone,
            request.Nome,
            request.ValorTotal,
            request.RealizadaEm,
            request.Produtos);

        await _mediator.Publish(evento);

        // 202: aceito para processamento assíncrono (contato é atualizado
        // de forma síncrona no handler, mas a notificação ao n8n é assíncrona via fila)
        return Accepted(new { message = "Venda registrada e notificação enfileirada." });
    }
}