using Microsoft.EntityFrameworkCore;
using ProdutosApi.Data;
using ProdutosApi.Dtos;
using ProdutosApi.Models;
using ProdutosApi.Services;
using Xunit;

namespace ProdutosApi.Tests;

[Collection(ServiceTests)]
public class CategoriaServiceTests
{
    private readonly ServiceTestDatabase _database;

    public CategoriaServiceTests(ServiceTestDatabase database)
    {
        _database = database;
    }

    [Fact]
    public async Task CriarAsync_DeveAdicionarCategoria_ERetornarComId()
    {
        await using var context = _database.Context;
        var service = new CategoriaService(context);
        var dto = new CategoriaCreateDto
        {
            Nome = "Eletrônicos",
            Descricao = "Produtos eletrônicos"
        };

        var resultado = await service.CriarAsync(dto);

        Assert.True(resultado.Id > 0);
        Assert.Equal("Eletrônicos", resultado.Nome);
        Assert.True(resultado.Ativo);
        Assert.Equal(1, await ((IQueryable<Categoria>)context.Categorias).CountAsync());
    }

    [Fact]
    public async Task CriarAsync_DeveRemoverEspacosDoNome()
    {
        await using var context = _database.Context;
        var service = new CategoriaService(context);

        var resultado = await service.CriarAsync(new CategoriaCreateDto
        {
            Nome = "  Informática  ",
            Descricao = "Hardware e periféricos"
        });

        Assert.Equal("Informática", resultado.Nome);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarNull_QuandoNaoEncontrado()
    {
        await using var context = _database.Context;
        var service = new CategoriaService(context);

        var resultado = await service.ObterPorIdAsync(999);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarCategoria_QuandoExiste()
    {
        await using var context = _database.Context;
        context.Categorias.Add(new Categoria { Nome = "Alimentos", Descricao = "Alimentos e bebidas" });
        await context.SaveChangesAsync();
        var service = new CategoriaService(context);

        var existente = await ((IQueryable<Categoria>)context.Categorias).FirstAsync();
        var resultado = await service.ObterPorIdAsync(existente.Id);

        Assert.NotNull(resultado);
        Assert.Equal("Alimentos", resultado!.Nome);
    }

    [Fact]
    public async Task ObterTodosAsync_DeveRetornarOrdenadoPorNome()
    {
        await using var context = _database.Context;
        context.Categorias.AddRange(
            new Categoria { Nome = "Zoo" },
            new Categoria { Nome = "Alpha" });
        await context.SaveChangesAsync();
        var service = new CategoriaService(context);

        var resultado = (await service.ObterTodosAsync()).ToList();

        Assert.Equal(2, resultado.Count);
        Assert.Equal("Alpha", resultado[0].Nome);
        Assert.Equal("Zoo", resultado[1].Nome);
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarCampos_ePreencherAtualizadoEm()
    {
        await using var context = _database.Context;
        context.Categorias.Add(new Categoria { Nome = "Brinquedos", Descricao = "Brinquedos infantis" });
        await context.SaveChangesAsync();
        var id = (await context.Categorias.FirstAsync()).Id;
        var service = new CategoriaService(context);

        var resultado = await service.AtualizarAsync(id, new CategoriaUpdateDto
        {
            Nome = "Brinquedos e Jogos",
            Descricao = "Brinquedos, jogos e puzzles",
            Ativo = false
        });

        Assert.NotNull(resultado);
        Assert.Equal("Brinquedos e Jogos", resultado!.Nome);
        Assert.False(resultado.Ativo);
        Assert.NotNull(resultado.AtualizadoEm);

        context.ChangeTracker.Clear();
        var atualizadoNoBanco = await ((IQueryable<Categoria>)context.Categorias).FirstAsync(c => c.Id == id);
        Assert.Equal("Brinquedos e Jogos", atualizadoNoBanco.Nome);
        Assert.Equal("Brinquedos, jogos e puzzles", atualizadoNoBanco.Descricao);
        Assert.False(atualizadoNoBanco.Ativo);
    }

    [Fact]
    public async Task AtualizarAsync_DeveRetornarNull_QuandoNaoEncontrado()
    {
        await using var context = _database.Context;
        var service = new CategoriaService(context);

        var resultado = await service.AtualizarAsync(123, new CategoriaUpdateDto
        {
            Nome = "Inexistente",
            Ativo = true
        });

        Assert.Null(resultado);
    }

    [Fact]
    public async Task RemoverAsync_DeveRemover_QuandoExiste()
    {
        await using var context = _database.Context;
        context.Categorias.Add(new Categoria { Nome = "Vestuário" });
        await context.SaveChangesAsync();
        var id = (await context.Categorias.FirstAsync()).Id;
        var service = new CategoriaService(context);

        var removido = await service.RemoverAsync(id);

        Assert.True(removido);
        Assert.Equal(0, await ((IQueryable<Categoria>)context.Categorias).CountAsync());
    }

    [Fact]
    public async Task RemoverAsync_DeveRetornarFalse_QuandoNaoEncontrado()
    {
        await using var context = _database.Context;
        var service = new CategoriaService(context);

        var removido = await service.RemoverAsync(404);

        Assert.False(removido);
    }
}
