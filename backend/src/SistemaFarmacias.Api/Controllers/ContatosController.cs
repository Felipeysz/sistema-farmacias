using Microsoft.AspNetCore.Mvc;
using SistemaFarmacias.Api.Auth;
using SistemaFarmacias.Application.Dtos;
using SistemaFarmacias.Application.Interfaces;
using SistemaFarmacias.Domain.Entities;

namespace SistemaFarmacias.Api.Controllers;

[ApiController]
[Route("api/n8n/crm/contatos")]
[ApiKeyAuth]
public class ContatosController : ControllerBase
{
    private readonly IContatoRepository _contatoRepository;

    public ContatosController(IContatoRepository contatoRepository)
    {
        _contatoRepository = contatoRepository;
    }

    [HttpPost("upsert")]
    public async Task<ActionResult<ContatoResponseDto>> Upsert([FromBody] ContatoUpsertRequestDto request)
    {
        var contato = await _contatoRepository.UpsertAsync(request.FarmaciaId, request.Telefone, request.Nome);

        return Ok(MapToResponse(contato));
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<ContatoResponseDto>> AtualizarAposVenda(
        Guid id,
        [FromBody] ContatoAtualizarAposVendaRequestDto request)
    {
        var contato = await _contatoRepository.AtualizarAposVendaAsync(
            id,
            request.FarmaciaId,
            request.DataUltimaCompra,
            request.ValorCompra);

        if (contato is null)
            return NotFound(new { message = "Contato não encontrado para essa farmácia." });

        return Ok(MapToResponse(contato));
    }

    /// <summary>
    /// Lista os contatos inativos de uma farmácia. Usado pelo Fluxo 2.2
    /// (schedule diário de reativação de inativos).
    /// </summary>
    [HttpGet("inativos")]
    public async Task<ActionResult<List<ContatoResponseDto>>> ListarInativos([FromQuery] Guid farmaciaId)
    {
        if (farmaciaId == Guid.Empty)
            return BadRequest(new { message = "farmaciaId é obrigatório." });

        var contatos = await _contatoRepository.GetInativosAsync(farmaciaId);

        return Ok(contatos.Select(MapToResponse));
    }

    private static ContatoResponseDto MapToResponse(Contato contato) => new()
    {
        Id = contato.Id,
        FarmaciaId = contato.FarmaciaId,
        Telefone = contato.Telefone,
        Nome = contato.Nome,
        UltimaInteracaoEm = contato.UltimaInteracaoEm,
        UltimaCompraEm = contato.UltimaCompraEm,
        TotalGasto = contato.TotalGasto,
        Status = contato.Status.ToString()
    };
}
