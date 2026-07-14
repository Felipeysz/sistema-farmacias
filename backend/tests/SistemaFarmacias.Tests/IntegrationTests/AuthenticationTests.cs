using System.Net;

namespace SistemaFarmacias.Tests.IntegrationTests;

[Collection("Integration")]
public class AuthenticationTests
{
    private readonly IntegrationTestFixture _fixture;

    public AuthenticationTests(IntegrationTestFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Endpoint_SemApiKey_Retorna401()
    {
        var client = _fixture.CreateUnauthenticatedClient();

        var response = await client.GetAsync($"/api/n8n/farmacias/by-whatsapp/qualquer-instance");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Endpoint_ComApiKeyInvalida_Retorna401()
    {
        var client = _fixture.CreateUnauthenticatedClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", "chave-errada");

        var response = await client.GetAsync($"/api/n8n/farmacias/by-whatsapp/qualquer-instance");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Endpoint_ComApiKeyValida_NaoRetorna401()
    {
        var client = _fixture.CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/n8n/farmacias/by-whatsapp/instance-que-nao-existe");

        // 404 é esperado (instance inexistente) — o importante aqui é que
        // a autenticação em si não bloqueou a requisição (não é 401).
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
