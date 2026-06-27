using Microsoft.EntityFrameworkCore;
using ProdutosApi.Data;
using ProdutosApi.Dtos;
using ProdutosApi.Models;
using ProdutosApi.Services;
using Xunit;

namespace ProdutosApi.Tests;

[Collection(ServiceTests.Name)]
public class PetServiceTests
{
    private readonly ServiceTestDatabase _database;

    public PetServiceTests(ServiceTestDatabase database)
    {
        _database = database;
    }

    [Fact]
    public async Task CriarAsync_DeveAdicionarPet_ERetornarComId()
    {
        await using var context = _database.Context;
        var service = new PetService(context);
        var dto = new PetCreateDto
        {
            Nome = "Rex",
            Raça = "Labrador",
            Cor = "Dourado"
        };

        var resultado = await service.CriarAsync(dto);

        Assert.True(resultado.Id > 0);
        Assert.Equal("Rex", resultado.Nome);
        Assert.Equal("Labrador", resultado.Raça);
        Assert.Equal("Dourado", resultado.Cor);
        Assert.True(resultado.Ativo);
        Assert.Equal(1, await ((IQueryable<Pet>)context.Pets).CountAsync());
    }

    [Fact]
    public async Task CriarAsync_DeveRemoverEspacosDoNome()
    {
        await using var context = _database.Context;
        var service = new PetService(context);

        var resultado = await service.CriarAsync(new PetCreateDto
        {
            Nome = "  Bidu  ",
            Raça = "Vira-lata",
            Cor = "Marrom"
        });

        Assert.Equal("Bidu", resultado.Nome);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarNull_QuandoNaoEncontrado()
    {
        await using var context = _database.Context;
        var service = new PetService(context);

        var resultado = await service.ObterPorIdAsync(999);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarPet_QuandoExiste()
    {
        await using var context = _database.Context;
        context.Pets.Add(new Pet { Nome = "Mimi", Raça = "Persa", Cor = "Branco" });
        await context.SaveChangesAsync();
        var service = new PetService(context);

        var existente = await ((IQueryable<Pet>)context.Pets).FirstAsync();
        var resultado = await service.ObterPorIdAsync(existente.Id);

        Assert.NotNull(resultado);
        Assert.Equal("Mimi", resultado!.Nome);
    }

    [Fact]
    public async Task ObterTodosAsync_DeveRetornarOrdenadoPorNome()
    {
        await using var context = _database.Context;
        context.Pets.AddRange(
            new Pet { Nome = "Zeca", Raça = "Calopsita", Cor = "Cinza" },
            new Pet { Nome = "Alpha", Raça = "Vira-lata", Cor = "Preto" });
        await context.SaveChangesAsync();
        var service = new PetService(context);

        var resultado = (await service.ObterTodosAsync()).ToList();

        Assert.Equal(2, resultado.Count);
        Assert.Equal("Alpha", resultado[0].Nome);
        Assert.Equal("Zeca", resultado[1].Nome);
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarCampos_ePreencherAtualizadoEm()
    {
        await using var context = _database.Context;
        context.Pets.Add(new Pet { Nome = "Thor", Raça = "Bulldog", Cor = "Preto" });
        await context.SaveChangesAsync();
        var id = (await context.Pets.FirstAsync()).Id;
        var service = new PetService(context);

        var resultado = await service.AtualizarAsync(id, new PetUpdateDto
        {
            Nome = "Thorzinho",
            Raça = "Bulldog Francês",
            Cor = "Bege",
            Ativo = false
        });

        Assert.NotNull(resultado);
        Assert.Equal("Thorzinho", resultado!.Nome);
        Assert.Equal("Bulldog Francês", resultado.Raça);
        Assert.Equal("Bege", resultado.Cor);
        Assert.False(resultado.Ativo);
        Assert.NotNull(resultado.AtualizadoEm);

        context.ChangeTracker.Clear();
        var atualizadoNoBanco = await ((IQueryable<Pet>)context.Pets).FirstAsync(p => p.Id == id);
        Assert.Equal("Thorzinho", atualizadoNoBanco.Nome);
        Assert.Equal("Bulldog Francês", atualizadoNoBanco.Raça);
        Assert.Equal("Bege", atualizadoNoBanco.Cor);
        Assert.False(atualizadoNoBanco.Ativo);
    }

    [Fact]
    public async Task AtualizarAsync_DeveRetornarNull_QuandoNaoEncontrado()
    {
        await using var context = _database.Context;
        var service = new PetService(context);

        var resultado = await service.AtualizarAsync(123, new PetUpdateDto
        {
            Nome = "Inexistente",
            Raça = "Desconhecida",
            Cor = "Desconhecida",
            Ativo = true
        });

        Assert.Null(resultado);
    }

    [Fact]
    public async Task RemoverAsync_DeveRemover_QuandoExiste()
    {
        await using var context = _database.Context;
        context.Pets.Add(new Pet { Nome = "Nina", Raça = "Poodle", Cor = "Rosa" });
        await context.SaveChangesAsync();
        var id = (await context.Pets.FirstAsync()).Id;
        var service = new PetService(context);

        var removido = await service.RemoverAsync(id);

        Assert.True(removido);
        Assert.Equal(0, await ((IQueryable<Pet>)context.Pets).CountAsync());
    }

    [Fact]
    public async Task RemoverAsync_DeveRetornarFalse_QuandoNaoEncontrado()
    {
        await using var context = _database.Context;
        var service = new PetService(context);

        var removido = await service.RemoverAsync(404);

        Assert.False(removido);
    }
}
