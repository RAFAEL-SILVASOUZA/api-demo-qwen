using Microsoft.EntityFrameworkCore;
using ProdutosApi.Data;
using ProdutosApi.Dtos;
using ProdutosApi.Models;
using ProdutosApi.Services;
using Xunit;

namespace ProdutosApi.Tests;

public class ProdutoServiceTests
{
    private static AppDbContext CriarContexto()
    {
        // Banco isolado por teste para evitar interferência entre cenários.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task CriarAsync_DeveAdicionarProduto_ERetornarComId()
    {
        await using var context = CriarContexto();
        var service = new ProdutoService(context);
        var dto = new ProdutoCreateDto
        {
            Nome = "Teclado Mecânico",
            Descricao = "Switch azul",
            Preco = 250.00m,
            QuantidadeEmEstoque = 10
        };

        var resultado = await service.CriarAsync(dto);

        Assert.True(resultado.Id > 0);
        Assert.Equal("Teclado Mecânico", resultado.Nome);
        Assert.True(resultado.Ativo);
        Assert.Equal(1, await context.Produtos.CountAsync());
    }

    [Fact]
    public async Task CriarAsync_DeveRemoverEspacosDoNome()
    {
        await using var context = CriarContexto();
        var service = new ProdutoService(context);

        var resultado = await service.CriarAsync(new ProdutoCreateDto
        {
            Nome = "  Mouse  ",
            Preco = 99.90m,
            QuantidadeEmEstoque = 5
        });

        Assert.Equal("Mouse", resultado.Nome);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarNull_QuandoNaoEncontrado()
    {
        await using var context = CriarContexto();
        var service = new ProdutoService(context);

        var resultado = await service.ObterPorIdAsync(999);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarProduto_QuandoExiste()
    {
        await using var context = CriarContexto();
        context.Produtos.Add(new Produto { Nome = "Monitor", Preco = 1200m, QuantidadeEmEstoque = 3 });
        await context.SaveChangesAsync();
        var service = new ProdutoService(context);

        var existente = await context.Produtos.FirstAsync();
        var resultado = await service.ObterPorIdAsync(existente.Id);

        Assert.NotNull(resultado);
        Assert.Equal("Monitor", resultado!.Nome);
    }

    [Fact]
    public async Task ObterTodosAsync_DeveRetornarOrdenadoPorNome()
    {
        await using var context = CriarContexto();
        context.Produtos.AddRange(
            new Produto { Nome = "Webcam", Preco = 200m, QuantidadeEmEstoque = 1 },
            new Produto { Nome = "Cabo HDMI", Preco = 30m, QuantidadeEmEstoque = 50 });
        await context.SaveChangesAsync();
        var service = new ProdutoService(context);

        var resultado = (await service.ObterTodosAsync()).ToList();

        Assert.Equal(2, resultado.Count);
        Assert.Equal("Cabo HDMI", resultado[0].Nome);
        Assert.Equal("Webcam", resultado[1].Nome);
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarCampos_ePreencherAtualizadoEm()
    {
        await using var context = CriarContexto();
        context.Produtos.Add(new Produto { Nome = "SSD", Preco = 300m, QuantidadeEmEstoque = 5 });
        await context.SaveChangesAsync();
        var id = (await context.Produtos.FirstAsync()).Id;
        var service = new ProdutoService(context);

        var resultado = await service.AtualizarAsync(id, new ProdutoUpdateDto
        {
            Nome = "SSD NVMe",
            Preco = 350m,
            QuantidadeEmEstoque = 8,
            Ativo = false
        });

        Assert.NotNull(resultado);
        Assert.Equal("SSD NVMe", resultado!.Nome);
        Assert.Equal(350m, resultado.Preco);
        Assert.False(resultado.Ativo);
        Assert.NotNull(resultado.AtualizadoEm);

        context.ChangeTracker.Clear();
        var atualizadoNoBanco = await context.Produtos.FirstAsync(p => p.Id == id);
        Assert.Equal("SSD NVMe", atualizadoNoBanco.Nome);
        Assert.Equal(350m, atualizadoNoBanco.Preco);
        Assert.Equal(8, atualizadoNoBanco.QuantidadeEmEstoque);
        Assert.False(atualizadoNoBanco.Ativo);
    }

    [Fact]
    public async Task AtualizarAsync_DeveRetornarNull_QuandoNaoEncontrado()
    {
        await using var context = CriarContexto();
        var service = new ProdutoService(context);

        var resultado = await service.AtualizarAsync(123, new ProdutoUpdateDto
        {
            Nome = "Inexistente",
            Preco = 10m,
            QuantidadeEmEstoque = 1,
            Ativo = true
        });

        Assert.Null(resultado);
    }

    [Fact]
    public async Task RemoverAsync_DeveRemover_QuandoExiste()
    {
        await using var context = CriarContexto();
        context.Produtos.Add(new Produto { Nome = "Pen Drive", Preco = 40m, QuantidadeEmEstoque = 20 });
        await context.SaveChangesAsync();
        var id = (await context.Produtos.FirstAsync()).Id;
        var service = new ProdutoService(context);

        var removido = await service.RemoverAsync(id);

        Assert.True(removido);
        Assert.Equal(0, await context.Produtos.CountAsync());
    }

    [Fact]
    public async Task RemoverAsync_DeveRetornarFalse_QuandoNaoEncontrado()
    {
        await using var context = CriarContexto();
        var service = new ProdutoService(context);

        var removido = await service.RemoverAsync(404);

        Assert.False(removido);
    }
}
