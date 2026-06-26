using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProdutosApi.Data;
using ProdutosApi.Dtos;
using ProdutosApi.Models;

namespace ProdutosApi.Tests.IntegrationTests;

public class ProdutosControllerIntegrationTests : IClassFixture<TestApplicationFactory>, IAsyncLifetime
{
    private readonly TestApplicationFactory _factory;
    private readonly HttpClient _client;

    public ProdutosControllerIntegrationTests(TestApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    public async Task InitializeAsync()
    {
        await using var context = _factory.Services.GetRequiredService<AppDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await using var context = _factory.Services.GetRequiredService<AppDbContext>();
        await context.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task PostCriar_DeveRetornar201_ePersistirNoPostgres()
    {
        var dto = new ProdutoCreateDto
        {
            Nome = "Teclado Mecânico",
            Descricao = "Switch azul",
            Preco = 250.00m,
            QuantidadeEmEstoque = 10
        };

        var response = await _client.PostAsJsonAsync("api/produtos", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var produto = await response.Content.ReadFromJsonAsync<ProdutoResponseDto>();

        produto.Should().NotBeNull();
        produto!.Id.Should().BePositive();
        produto.Nome.Should().Be("Teclado Mecânico");
        produto.Ativo.Should().BeTrue();

        await using var context = _factory.Services.GetRequiredService<AppDbContext>();
        (await ((IQueryable<Produto>)context.Produtos).CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task GetTodos_DeveRetornar200_eListaOrdenada()
    {
        await using var context = _factory.Services.GetRequiredService<AppDbContext>();
        context.Produtos.AddRange(
            new Produto { Nome = "Webcam", Preco = 200m, QuantidadeEmEstoque = 1 },
            new Produto { Nome = "Cabo HDMI", Preco = 30m, QuantidadeEmEstoque = 50 });
        await context.SaveChangesAsync();

        var response = await _client.GetAsync("api/produtos");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var produtos = await response.Content.ReadFromJsonAsync<List<ProdutoResponseDto>>();

        produtos.Should().HaveCount(2);
        produtos![0].Nome.Should().Be("Cabo HDMI");
        produtos[1].Nome.Should().Be("Webcam");
    }

    [Fact]
    public async Task GetPorId_DeveRetornar200_QuandoExiste()
    {
        await using var context = _factory.Services.GetRequiredService<AppDbContext>();
        context.Produtos.Add(new Produto { Nome = "Monitor", Preco = 1200m, QuantidadeEmEstoque = 3 });
        await context.SaveChangesAsync();

        var produto = await ((IQueryable<Produto>)context.Produtos).FirstAsync();
        var response = await _client.GetAsync($"api/produtos/{produto.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<ProdutoResponseDto>();

        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(produto.Id);
        resultado.Nome.Should().Be("Monitor");
    }

    [Fact]
    public async Task GetPorId_DeveRetornar404_QuandoNaoExiste()
    {
        var response = await _client.GetAsync("api/produtos/999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PutAtualizar_DeveRetornar200_eAtualizarNoPostgres()
    {
        await using var context = _factory.Services.GetRequiredService<AppDbContext>();
        context.Produtos.Add(new Produto { Nome = "SSD", Preco = 300m, QuantidadeEmEstoque = 5 });
        await context.SaveChangesAsync();

        var produto = await ((IQueryable<Produto>)context.Produtos).FirstAsync();
        var dto = new ProdutoUpdateDto
        {
            Nome = "SSD NVMe",
            Preco = 350m,
            QuantidadeEmEstoque = 8,
            Ativo = false
        };

        var response = await _client.PutAsJsonAsync($"api/produtos/{produto.Id}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<ProdutoResponseDto>();

        resultado.Should().NotBeNull();
        resultado!.Nome.Should().Be("SSD NVMe");
        resultado.Preco.Should().Be(350m);
        resultado.QuantidadeEmEstoque.Should().Be(8);
        resultado.Ativo.Should().BeFalse();
        resultado.AtualizadoEm.Should().NotBeNull();

        context.ChangeTracker.Clear();
        var persistido = await ((IQueryable<Produto>)context.Produtos).FirstAsync(p => p.Id == produto.Id);
        persistido.Nome.Should().Be("SSD NVMe");
    }

    [Fact]
    public async Task DeleteRemover_DeveRetornar204_eRemoverDoPostgres()
    {
        await using var context = _factory.Services.GetRequiredService<AppDbContext>();
        context.Produtos.Add(new Produto { Nome = "Pen Drive", Preco = 40m, QuantidadeEmEstoque = 20 });
        await context.SaveChangesAsync();

        var produto = await ((IQueryable<Produto>)context.Produtos).FirstAsync();
        var response = await _client.DeleteAsync($"api/produtos/{produto.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await ((IQueryable<Produto>)context.Produtos).CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task DeleteRemover_DeveRetornar404_QuandoNaoExiste()
    {
        var response = await _client.DeleteAsync("api/produtos/999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
