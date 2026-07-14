using System.Net;
using System.Net.Http.Json;
using SistemaFarmacias.Application.Dtos;
using SistemaFarmacias.Domain.Entities;

namespace SistemaFarmacias.Tests.IntegrationTests;

[Collection("Integration")]
public class ContatosControllerTests
{
    private readonly IntegrationTestFixture _fixture;

    public ContatosControllerTests(IntegrationTestFixture fixture) => _fixture = fixture;

    /// <summary>
    /// Cria uma Farmacia real no banco e retorna o Id dela — necessário porque
    /// "contatos" tem uma foreign key para "farmacias", então qualquer teste
    /// que insira um contato precisa de uma farmácia existente primeiro.
    /// </summary>
    private async Task<Guid> SeedFarmaciaAsync()
    {
        await using var context = await _fixture.CreateDbContextAsync();
        var farmaciaId = Guid.NewGuid();
        context.Farmacias.Add(new Farmacia { Id = farmaciaId, Nome = "Farmácia Teste Contatos" });
        await context.SaveChangesAsync();
        return farmaciaId;
    }

    [Fact]
    public async Task Upsert_CriaNovoContato_ComStatusAtivo()
    {
        // Arrange
        var client = _fixture.CreateAuthenticatedClient();
        var farmaciaId = await SeedFarmaciaAsync();
        var request = new ContatoUpsertRequestDto
        {
            FarmaciaId = farmaciaId,
            Telefone = $"55119{Random.Shared.Next(10000000, 99999999)}",
            Nome = "Cliente Integração"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/n8n/crm/contatos/upsert", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ContatoResponseDto>();
        Assert.Equal("Ativo", body!.Status);
        Assert.Equal(request.Telefone, body.Telefone);
    }

    [Fact]
    public async Task Upsert_ChamadoDuasVezesComMesmoTelefone_NaoDuplicaContato()
    {
        // Arrange — valida o índice único (FarmaciaId, Telefone) contra o Postgres real
        var client = _fixture.CreateAuthenticatedClient();
        var farmaciaId = await SeedFarmaciaAsync();
        var telefone = $"55119{Random.Shared.Next(10000000, 99999999)}";

        // Act
        var primeira = await client.PostAsJsonAsync("/api/n8n/crm/contatos/upsert",
            new ContatoUpsertRequestDto { FarmaciaId = farmaciaId, Telefone = telefone, Nome = "Nome 1" });
        var segunda = await client.PostAsJsonAsync("/api/n8n/crm/contatos/upsert",
            new ContatoUpsertRequestDto { FarmaciaId = farmaciaId, Telefone = telefone, Nome = "Nome 2" });

        // Assert
        var corpo1 = await primeira.Content.ReadFromJsonAsync<ContatoResponseDto>();
        var corpo2 = await segunda.Content.ReadFromJsonAsync<ContatoResponseDto>();
        Assert.Equal(corpo1!.Id, corpo2!.Id);
        Assert.Equal("Nome 2", corpo2.Nome);
    }

    [Fact]
    public async Task AtualizarAposVenda_SomaTotalGastoEReativaContato()
    {
        // Arrange
        var client = _fixture.CreateAuthenticatedClient();
        var farmaciaId = await SeedFarmaciaAsync();
        var upsertResponse = await client.PostAsJsonAsync("/api/n8n/crm/contatos/upsert",
            new ContatoUpsertRequestDto
            {
                FarmaciaId = farmaciaId,
                Telefone = $"55119{Random.Shared.Next(10000000, 99999999)}",
                Nome = "Cliente"
            });
        var contato = await upsertResponse.Content.ReadFromJsonAsync<ContatoResponseDto>();

        // Act — duas "vendas" seguidas
        await client.PatchAsJsonAsync($"/api/n8n/crm/contatos/{contato!.Id}",
            new ContatoAtualizarAposVendaRequestDto { FarmaciaId = farmaciaId, DataUltimaCompra = DateTime.UtcNow, ValorCompra = 50m });
        var segunda = await client.PatchAsJsonAsync($"/api/n8n/crm/contatos/{contato.Id}",
            new ContatoAtualizarAposVendaRequestDto { FarmaciaId = farmaciaId, DataUltimaCompra = DateTime.UtcNow, ValorCompra = 30m });

        // Assert
        segunda.EnsureSuccessStatusCode();
        var body = await segunda.Content.ReadFromJsonAsync<ContatoResponseDto>();
        Assert.Equal(80m, body!.TotalGasto);
    }

    [Fact]
    public async Task AtualizarAposVenda_ComFarmaciaIdDeOutroTenant_Retorna404()
    {
        // Arrange — isolamento entre tenants, contra Postgres real
        var client = _fixture.CreateAuthenticatedClient();
        var farmaciaA = await SeedFarmaciaAsync();
        var farmaciaB = Guid.NewGuid(); // não precisa existir de verdade: nunca é inserida, só usada no filtro do PATCH
        var upsertResponse = await client.PostAsJsonAsync("/api/n8n/crm/contatos/upsert",
            new ContatoUpsertRequestDto
            {
                FarmaciaId = farmaciaA,
                Telefone = $"55119{Random.Shared.Next(10000000, 99999999)}",
                Nome = "Cliente"
            });
        var contato = await upsertResponse.Content.ReadFromJsonAsync<ContatoResponseDto>();

        // Act — tenta atualizar usando o farmaciaId de outra farmácia
        var response = await client.PatchAsJsonAsync($"/api/n8n/crm/contatos/{contato!.Id}",
            new ContatoAtualizarAposVendaRequestDto { FarmaciaId = farmaciaB, DataUltimaCompra = DateTime.UtcNow, ValorCompra = 10m });

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetInativos_RetornaApenasContatosInativosDaFarmaciaCorreta()
    {
        // Arrange
        var client = _fixture.CreateAuthenticatedClient();
        var farmaciaId = await SeedFarmaciaAsync();

        var upsertResponse = await client.PostAsJsonAsync("/api/n8n/crm/contatos/upsert",
            new ContatoUpsertRequestDto
            {
                FarmaciaId = farmaciaId,
                Telefone = $"55119{Random.Shared.Next(10000000, 99999999)}",
                Nome = "Inativo"
            });
        var contato = await upsertResponse.Content.ReadFromJsonAsync<ContatoResponseDto>();

        await using var context = await _fixture.CreateDbContextAsync();
        var entidade = await context.Contatos.FindAsync(contato!.Id);
        entidade!.Status = StatusContato.Inativo;
        await context.SaveChangesAsync();

        // Act
        var response = await client.GetAsync($"/api/n8n/crm/contatos/inativos?farmaciaId={farmaciaId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var lista = await response.Content.ReadFromJsonAsync<List<ContatoResponseDto>>();
        Assert.Single(lista!);
        Assert.Equal(contato.Id, lista![0].Id);
    }
}
