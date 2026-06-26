using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProdutosApi.Data;
using ProdutosApi.Dtos;
using ProdutosApi.Models;

namespace ProdutosApi.Tests.IntegrationTests;

public class CategoriasControllerIntegrationTests : IClassFixture<TestApplicationFactory>, IAsyncLifetime
{
    private readonly TestApplicationFactory _factory;
    private readonly HttpClient _client;

    public CategoriasControllerIntegrationTests(TestApplicationFactory factory)
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
        var dto = new CategoriaCreateDto
        {
            Nome = "Eletrônicos",
            Descricao = "Produtos eletrônicos"
        };

        var response = await _client.PostAsJsonAsync("api/categorias", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var categoria = await response.Content.ReadFromJsonAsync<CategoriaResponseDto>();

        categoria.Should().NotBeNull();
        categoria!.Id.Should().BePositive();
        categoria.Nome.Should().Be("Eletrônicos");
        categoria.Ativo.Should().BeTrue();

        await using var context = _factory.Services.GetRequiredService<AppDbContext>();
        (await ((IQueryable<Categoria>)context.Categorias).CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task GetTodos_DeveRetornar200_eListaOrdenada()
    {
        await using var context = _factory.Services.GetRequiredService<AppDbContext>();
        context.Categorias.AddRange(
            new Categoria { Nome = "Zoo" },
            new Categoria { Nome = "Alpha" });
        await context.SaveChangesAsync();

        var response = await _client.GetAsync("api/categorias");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var categorias = await response.Content.ReadFromJsonAsync<List<CategoriaResponseDto>>();

        categorias.Should().HaveCount(2);
        categorias![0].Nome.Should().Be("Alpha");
        categorias[1].Nome.Should().Be("Zoo");
    }

    [Fact]
    public async Task GetPorId_DeveRetornar200_QuandoExiste()
    {
        await using var context = _factory.Services.GetRequiredService<AppDbContext>();
        context.Categorias.Add(new Categoria { Nome = "Alimentos", Descricao = "Alimentos e bebidas" });
        await context.SaveChangesAsync();

        var categoria = await ((IQueryable<Categoria>)context.Categorias).FirstAsync();
        var response = await _client.GetAsync($"api/categorias/{categoria.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<CategoriaResponseDto>();

        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(categoria.Id);
        resultado.Nome.Should().Be("Alimentos");
    }

    [Fact]
    public async Task GetPorId_DeveRetornar404_QuandoNaoExiste()
    {
        var response = await _client.GetAsync("api/categorias/999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PutAtualizar_DeveRetornar200_eAtualizarNoPostgres()
    {
        await using var context = _factory.Services.GetRequiredService<AppDbContext>();
        context.Categorias.Add(new Categoria { Nome = "Brinquedos", Descricao = "Brinquedos infantis" });
        await context.SaveChangesAsync();

        var categoria = await ((IQueryable<Categoria>)context.Categorias).FirstAsync();
        var dto = new CategoriaUpdateDto
        {
            Nome = "Brinquedos e Jogos",
            Descricao = "Brinquedos, jogos e puzzles",
            Ativo = false
        };

        var response = await _client.PutAsJsonAsync($"api/categorias/{categoria.Id}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<CategoriaResponseDto>();

        resultado.Should().NotBeNull();
        resultado!.Nome.Should().Be("Brinquedos e Jogos");
        resultado.Ativo.Should().BeFalse();
        resultado.AtualizadoEm.Should().NotBeNull();

        context.ChangeTracker.Clear();
        var persistido = await ((IQueryable<Categoria>)context.Categorias).FirstAsync(c => c.Id == categoria.Id);
        persistido.Nome.Should().Be("Brinquedos e Jogos");
    }

    [Fact]
    public async Task DeleteRemover_DeveRetornar204_eRemoverDoPostgres()
    {
        await using var context = _factory.Services.GetRequiredService<AppDbContext>();
        context.Categorias.Add(new Categoria { Nome = "Vestuário" });
        await context.SaveChangesAsync();

        var categoria = await ((IQueryable<Categoria>)context.Categorias).FirstAsync();
        var response = await _client.DeleteAsync($"api/categorias/{categoria.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await ((IQueryable<Categoria>)context.Categorias).CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task DeleteRemover_DeveRetornar404_QuandoNaoExiste()
    {
        var response = await _client.DeleteAsync("api/categorias/999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
