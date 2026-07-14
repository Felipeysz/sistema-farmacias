using System.Net;
using System.Net.Http.Json;
using SistemaFarmacias.Application.Dtos;
using SistemaFarmacias.Domain.Entities;

namespace SistemaFarmacias.Tests.IntegrationTests;

[Collection("Integration")]
public class FarmaciasControllerTests
{
    private readonly IntegrationTestFixture _fixture;

    public FarmaciasControllerTests(IntegrationTestFixture fixture) => _fixture = fixture;

    private async Task<(Guid FarmaciaId, string WhatsappNumberId)> SeedFarmaciaAsync()
    {
        await using var context = await _fixture.CreateDbContextAsync();

        var farmaciaId = Guid.NewGuid();
        var whatsappNumberId = $"instance-{Guid.NewGuid():N}";

        context.Farmacias.Add(new Farmacia { Id = farmaciaId, Nome = "Farmácia Integração" });
        context.WhatsappConfigs.Add(new WhatsappConfig
        {
            Id = Guid.NewGuid(),
            FarmaciaId = farmaciaId,
            WhatsappNumberId = whatsappNumberId,
            NomeExibicao = "Farmácia Integração",
            HorarioFuncionamento = "8h às 20h",
            Endereco = "Rua Teste, 100",
            MensagemSaudacao = "Olá!"
        });
        await context.SaveChangesAsync();

        return (farmaciaId, whatsappNumberId);
    }

    [Fact]
    public async Task GetByWhatsapp_QuandoExiste_RetornaDadosDaFarmacia()
    {
        // Arrange
        var (farmaciaId, whatsappNumberId) = await SeedFarmaciaAsync();
        var client = _fixture.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync($"/api/n8n/farmacias/by-whatsapp/{whatsappNumberId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<FarmaciaByWhatsappResponseDto>();
        Assert.Equal(farmaciaId, body!.Id);
        Assert.Equal("Farmácia Integração", body.NomeExibicao);
    }

    [Fact]
    public async Task GetByWhatsapp_QuandoNaoExiste_Retorna404()
    {
        var client = _fixture.CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/n8n/farmacias/by-whatsapp/nao-existe");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetWhatsappConfig_QuandoExiste_RetornaWhatsappNumberId()
    {
        // Arrange — cobre o fluxo inverso usado pela LASDWAS-20
        var (farmaciaId, whatsappNumberId) = await SeedFarmaciaAsync();
        var client = _fixture.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync($"/api/n8n/farmacias/{farmaciaId}/whatsapp-config");

        // Assert
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<WhatsappConfigResponseDto>();
        Assert.Equal(whatsappNumberId, body!.WhatsappNumberId);
    }

    [Fact]
    public async Task GetAll_RetornaFarmaciaRecemCriada()
    {
        // Arrange
        var (farmaciaId, _) = await SeedFarmaciaAsync();
        var client = _fixture.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/n8n/farmacias");

        // Assert
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<List<FarmaciaListItemResponseDto>>();
        Assert.Contains(body!, f => f.Id == farmaciaId);
    }
}
