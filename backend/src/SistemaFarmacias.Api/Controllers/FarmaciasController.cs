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

    /// <summary>
    /// Lista todas as farmácias com WhatsApp configurado. Usado pelo Fluxo 2.2
    /// para iterar sobre todas as farmácias sem depender de um id fixo.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<FarmaciaListItemResponseDto>>> GetAll()
    {
        var configs = await _farmaciaRepository.GetAllAsync();

        var response = configs.Select(c => new FarmaciaListItemResponseDto
        {
            Id = c.FarmaciaId,
            WhatsappNumberId = c.WhatsappNumberId,
            NomeExibicao = c.NomeExibicao
        });

        return Ok(response);
    }
}
