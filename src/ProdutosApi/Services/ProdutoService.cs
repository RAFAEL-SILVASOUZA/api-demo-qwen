using Microsoft.EntityFrameworkCore;
using ProdutosApi.Data;
using ProdutosApi.Dtos;
using ProdutosApi.Models;

namespace ProdutosApi.Services;

public class ProdutoService : IProdutoService
{
    private readonly AppDbContext _context;

    public ProdutoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProdutoResponseDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var produtos = await _context.Produtos
            .AsNoTracking()
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);

        return produtos.Select(MapToResponse);
    }

    public async Task<ProdutoResponseDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var produto = await _context.Produtos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return produto is null ? null : MapToResponse(produto);
    }

    public async Task<ProdutoResponseDto> CriarAsync(ProdutoCreateDto dto, CancellationToken cancellationToken = default)
    {
        var produto = new Produto
        {
            Nome = dto.Nome.Trim(),
            Descricao = dto.Descricao?.Trim(),
            Preco = dto.Preco,
            QuantidadeEmEstoque = dto.QuantidadeEmEstoque,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponse(produto);
    }

    public async Task<ProdutoResponseDto?> AtualizarAsync(int id, ProdutoUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (produto is null)
        {
            return null;
        }

        produto.Nome = dto.Nome.Trim();
        produto.Descricao = dto.Descricao?.Trim();
        produto.Preco = dto.Preco;
        produto.QuantidadeEmEstoque = dto.QuantidadeEmEstoque;
        produto.Ativo = dto.Ativo;
        produto.AtualizadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponse(produto);
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (produto is null)
        {
            return false;
        }

        _context.Produtos.Remove(produto);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static ProdutoResponseDto MapToResponse(Produto produto) => new()
    {
        Id = produto.Id,
        Nome = produto.Nome,
        Descricao = produto.Descricao,
        Preco = produto.Preco,
        QuantidadeEmEstoque = produto.QuantidadeEmEstoque,
        Ativo = produto.Ativo,
        CriadoEm = produto.CriadoEm,
        AtualizadoEm = produto.AtualizadoEm
    };
}
