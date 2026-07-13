using Microsoft.AspNetCore.Mvc;
using SistemaFarmacias.Application.Dtos;
using SistemaFarmacias.Application.Interfaces;

namespace SistemaFarmacias.Api.Controllers;

[ApiController]
[Route("api/n8n/farmacias")]
public class FarmaciasController : ControllerBase
{
    private readonly IFarmaciaRepository _farmaciaRepository;

    public FarmaciasController(IFarmaciaRepository farmaciaRepository)
    {
        _farmaciaRepository = farmaciaRepository;
    }

    [HttpGet("by-whatsapp/{id}")]
    public async Task<ActionResult<FarmaciaByWhatsappResponseDto>> GetByWhatsapp(string id)
    {
        var config = await _farmaciaRepository.GetByWhatsappNumberIdAsync(id);

        if (config is null)
            return NotFound(new { message = "Nenhuma farmácia encontrada para esse número de WhatsApp." });

        var response = new FarmaciaByWhatsappResponseDto
        {
            Id = config.FarmaciaId,
            NomeExibicao = config.NomeExibicao,
            HorarioFuncionamento = config.HorarioFuncionamento,
            Endereco = config.Endereco,
            MensagemSaudacao = config.MensagemSaudacao
        };

        return Ok(response);
    }

    /// <summary>
    /// Usado pelo Fluxo 2.1 (venda registrada), que só tem o FarmaciaId
    /// disponível e precisa descobrir o instance id do Evolution API
    /// para enviar a mensagem de agradecimento.
    /// </summary>
    [HttpGet("{farmaciaId:guid}/whatsapp-config")]
    public async Task<ActionResult<WhatsappConfigResponseDto>> GetWhatsappConfig(Guid farmaciaId)
    {
        var config = await _farmaciaRepository.GetByFarmaciaIdAsync(farmaciaId);

        if (config is null)
            return NotFound(new { message = "Nenhuma configuração de WhatsApp encontrada para essa farmácia." });

        var response = new WhatsappConfigResponseDto
        {
            FarmaciaId = config.FarmaciaId,
            WhatsappNumberId = config.WhatsappNumberId,
            NomeExibicao = config.NomeExibicao
        };

        return Ok(response);
    }
}
