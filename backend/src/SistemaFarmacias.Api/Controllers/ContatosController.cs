using Microsoft.AspNetCore.Mvc;
using SistemaFarmacias.Api.Auth;
using SistemaFarmacias.Application.Dtos;
using SistemaFarmacias.Application.Interfaces;

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

        var response = new ContatoResponseDto
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

        return Ok(response);
    }
}
