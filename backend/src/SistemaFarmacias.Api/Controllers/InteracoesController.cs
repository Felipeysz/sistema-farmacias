using Microsoft.AspNetCore.Mvc;
using SistemaFarmacias.Api.Auth;
using SistemaFarmacias.Application.Dtos;
using SistemaFarmacias.Application.Interfaces;

namespace SistemaFarmacias.Api.Controllers;

[ApiController]
[Route("api/n8n/crm/interacoes")]
[ApiKeyAuth]
public class InteracoesController : ControllerBase
{
    private readonly IInteracaoRepository _interacaoRepository;

    public InteracoesController(IInteracaoRepository interacaoRepository)
    {
        _interacaoRepository = interacaoRepository;
    }

    [HttpPost]
    public async Task<ActionResult<InteracaoResponseDto>> Create([FromBody] InteracaoCreateRequestDto request)
    {
        var interacao = await _interacaoRepository.CreateAsync(
            request.FarmaciaId,
            request.ContatoId,
            request.MensagemRecebida,
            request.MensagemEnviada,
            request.IntencaoDetectada);

        if (interacao is null)
            return NotFound(new { message = "Contato não encontrado para essa farmácia." });

        var response = new InteracaoResponseDto
        {
            Id = interacao.Id,
            FarmaciaId = interacao.FarmaciaId,
            ContatoId = interacao.ContatoId,
            Canal = interacao.Canal.ToString(),
            MensagemRecebida = interacao.MensagemRecebida,
            MensagemEnviada = interacao.MensagemEnviada,
            IntencaoDetectada = interacao.IntencaoDetectada,
            CriadoEm = interacao.CriadoEm
        };

        return CreatedAtAction(nameof(Create), new { id = response.Id }, response);
    }
}
