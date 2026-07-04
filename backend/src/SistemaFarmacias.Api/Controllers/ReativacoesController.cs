using Microsoft.AspNetCore.Mvc;
using SistemaFarmacias.Application.Dtos;
using SistemaFarmacias.Application.Interfaces;

namespace SistemaFarmacias.Api.Controllers;

[ApiController]
[Route("api/n8n/crm/reativacoes")]
public class ReativacoesController : ControllerBase
{
    private readonly IReativacaoRepository _reativacaoRepository;

    public ReativacoesController(IReativacaoRepository reativacaoRepository)
    {
        _reativacaoRepository = reativacaoRepository;
    }

    [HttpPost]
    public async Task<ActionResult<ReativacaoResponseDto>> Create([FromBody] ReativacaoCreateRequestDto request)
    {
        var reativacao = await _reativacaoRepository.CreateAsync(request.FarmaciaId, request.ContatoId);

        if (reativacao is null)
            return NotFound(new { message = "Contato não encontrado para essa farmácia." });

        var response = new ReativacaoResponseDto
        {
            Id = reativacao.Id,
            FarmaciaId = reativacao.FarmaciaId,
            ContatoId = reativacao.ContatoId,
            EnviadoEm = reativacao.EnviadoEm
        };

        return CreatedAtAction(nameof(Create), new { id = response.Id }, response);
    }
}
