using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using SistemaFarmacias.Application.Dtos;
using SistemaFarmacias.Domain.Entities;

namespace SistemaFarmacias.Tests.IntegrationTests;

[Collection("Integration")]
public class VendasIdempotenciaConcorrenciaTests
{
    private readonly IntegrationTestFixture _fixture;

    public VendasIdempotenciaConcorrenciaTests(IntegrationTestFixture fixture) => _fixture = fixture;

    private async Task<Guid> SeedFarmaciaAsync()
    {
        await using var context = await _fixture.CreateDbContextAsync();
        var farmaciaId = Guid.NewGuid();
        context.Farmacias.Add(new Farmacia { Id = farmaciaId, Nome = "Farmácia Idempotência" });
        await context.SaveChangesAsync();
        return farmaciaId;
    }

    private static VendaRegistradaRequestDto CriarRequest(Guid vendaId, Guid farmaciaId, string telefone, decimal valor) => new()
    {
        VendaId = vendaId,
        FarmaciaId = farmaciaId,
        Telefone = telefone,
        Nome = "Cliente Concorrência",
        ValorTotal = valor,
        RealizadaEm = DateTime.UtcNow,
        Produtos = new List<ItemVendaDto> { new() { NomeProduto = "Dipirona", Quantidade = 1 } }
    };

    /// <summary>
    /// Espera o TotalGasto convergir para o valor esperado, com um pequeno
    /// timeout — necessário porque a atualização de contato acontece dentro
    /// do handler MediatR.Publish, que roda de forma assíncrona em relação
    /// à resposta HTTP 202 (Accepted) do controller.
    /// </summary>
    private async Task<Contato?> AguardarContatoAsync(Guid farmaciaId, string telefone, TimeSpan timeout)
    {
        var prazoFinal = DateTime.UtcNow + timeout;
        await using var context = await _fixture.CreateDbContextAsync();

        while (DateTime.UtcNow < prazoFinal)
        {
            var contato = await context.Contatos
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.FarmaciaId == farmaciaId && c.Telefone == telefone);

            if (contato is not null)
                return contato;

            await Task.Delay(100);
        }

        return null;
    }

    [Fact]
    public async Task Idempotencia_MesmoVendaIdEnviadoDuasVezesSequencialmente_NaoDuplicaEfeito()
    {
        // Arrange
        var client = _fixture.CreateAuthenticatedClient();
        var farmaciaId = await SeedFarmaciaAsync();
        var telefone = $"55119{Random.Shared.Next(10000000, 99999999)}";
        var vendaId = Guid.NewGuid();
        var request = CriarRequest(vendaId, farmaciaId, telefone, 50m);

        // Act — envia a MESMA venda (mesmo VendaId) duas vezes seguidas,
        // simulando um retry do PDV por timeout de rede
        var primeira = await client.PostAsJsonAsync("/api/vendas/registrar", request);
        primeira.EnsureSuccessStatusCode();
        await Task.Delay(500); // dá tempo do handler processar a primeira antes da segunda

        var segunda = await client.PostAsJsonAsync("/api/vendas/registrar", request);
        segunda.EnsureSuccessStatusCode();
        await Task.Delay(500);

        // Assert — TotalGasto reflete UMA venda de 50, não duas (100)
        var contato = await AguardarContatoAsync(farmaciaId, telefone, TimeSpan.FromSeconds(5));
        Assert.NotNull(contato);
        Assert.Equal(50m, contato!.TotalGasto);
    }

    [Fact]
    public async Task Idempotencia_MesmoVendaIdEnviadoEmParalelo_ProcessaApenasUmaVez()
    {
        // Arrange — cenário mais rigoroso que o anterior: as duas requisições
        // com o MESMO VendaId chegam ao mesmo tempo, testando a trava de
        // concorrência real (PK do Postgres), não só o caminho sequencial
        var client = _fixture.CreateAuthenticatedClient();
        var farmaciaId = await SeedFarmaciaAsync();
        var telefone = $"55119{Random.Shared.Next(10000000, 99999999)}";
        var vendaId = Guid.NewGuid();
        var request = CriarRequest(vendaId, farmaciaId, telefone, 40m);

        // Act — dispara as duas chamadas em paralelo de propósito
        var respostas = await Task.WhenAll(
            client.PostAsJsonAsync("/api/vendas/registrar", request),
            client.PostAsJsonAsync("/api/vendas/registrar", request));

        foreach (var resposta in respostas)
            resposta.EnsureSuccessStatusCode(); // ambas retornam 202, mesmo a "duplicata"

        // Assert — mesmo com duas requisições simultâneas, o efeito só aconteceu uma vez
        var contato = await AguardarContatoAsync(farmaciaId, telefone, TimeSpan.FromSeconds(5));
        Assert.NotNull(contato);
        Assert.Equal(40m, contato!.TotalGasto);
    }

    [Fact]
    public async Task Concorrencia_VendasDiferentesDoMesmoContatoEmParalelo_SomaCorretamenteSemPerderAtualizacao()
    {
        // Arrange — N vendas DISTINTAS (VendaId diferente cada) do mesmo
        // contato, disparadas ao mesmo tempo. Testa o lost-update clássico:
        // sem UPDATE atômico, duas leituras concorrentes do mesmo TotalGasto
        // inicial fariam uma sobrescrever a outra.
        var client = _fixture.CreateAuthenticatedClient();
        var farmaciaId = await SeedFarmaciaAsync();
        var telefone = $"55119{Random.Shared.Next(10000000, 99999999)}";

        const int quantidadeVendas = 10;
        const decimal valorPorVenda = 10m;

        var requests = Enumerable.Range(0, quantidadeVendas)
            .Select(_ => CriarRequest(Guid.NewGuid(), farmaciaId, telefone, valorPorVenda))
            .ToList();

        // Act — todas disparadas em paralelo, de propósito
        var respostas = await Task.WhenAll(
            requests.Select(r => client.PostAsJsonAsync("/api/vendas/registrar", r)));

        foreach (var resposta in respostas)
            resposta.EnsureSuccessStatusCode();

        // Assert — soma final tem que bater com TODAS as vendas, nenhuma perdida
        var valorEsperado = quantidadeVendas * valorPorVenda;
        Contato? contato = null;

        // Aguarda um pouco mais aqui: 10 handlers concorrentes competindo
        // pelo mesmo contato podem demorar mais que o cenário de 1-2 vendas
        var prazoFinal = DateTime.UtcNow + TimeSpan.FromSeconds(10);
        await using var context = await _fixture.CreateDbContextAsync();
        while (DateTime.UtcNow < prazoFinal)
        {
            contato = await context.Contatos
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.FarmaciaId == farmaciaId && c.Telefone == telefone);

            if (contato is not null && contato.TotalGasto == valorEsperado)
                break;

            await Task.Delay(200);
        }

        Assert.NotNull(contato);
        Assert.Equal(valorEsperado, contato!.TotalGasto);
    }
}
