using Microsoft.EntityFrameworkCore;
using ProdutosApi.Data;
using ProdutosApi.Dtos;
using ProdutosApi.Models;

namespace ProdutosApi.Services;

public class CategoriaService : ICategoriaService
{
    private readonly AppDbContext _context;

    public CategoriaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoriaResponseDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var categorias = await _context.Categorias
            .AsNoTracking()
            .OrderBy(c => c.Nome)
            .ToListAsync(cancellationToken);

        return categorias.Select(MapToResponse);
    }

    public async Task<CategoriaResponseDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var categoria = await _context.Categorias
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        return categoria is null ? null : MapToResponse(categoria);
    }

    public async Task<CategoriaResponseDto> CriarAsync(CategoriaCreateDto dto, CancellationToken cancellationToken = default)
    {
        var categoria = new Categoria
        {
            Nome = dto.Nome.Trim(),
            Descricao = dto.Descricao?.Trim(),
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponse(categoria);
    }

    public async Task<CategoriaResponseDto?> AtualizarAsync(int id, CategoriaUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var categoria = await _context.Categorias.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (categoria is null)
        {
            return null;
        }

        categoria.Nome = dto.Nome.Trim();
        categoria.Descricao = dto.Descricao?.Trim();
        categoria.Ativo = dto.Ativo;
        categoria.AtualizadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponse(categoria);
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var categoria = await _context.Categorias.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (categoria is null)
        {
            return false;
        }

        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static CategoriaResponseDto MapToResponse(Categoria categoria) => new()
    {
        Id = categoria.Id,
        Nome = categoria.Nome,
        Descricao = categoria.Descricao,
        Ativo = categoria.Ativo,
        CriadoEm = categoria.CriadoEm,
        AtualizadoEm = categoria.AtualizadoEm
    };
}
