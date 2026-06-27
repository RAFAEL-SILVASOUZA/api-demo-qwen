using Microsoft.EntityFrameworkCore;
using ProdutosApi.Data;
using ProdutosApi.Dtos;
using ProdutosApi.Models;

namespace ProdutosApi.Services;

public class PetService : IPetService
{
    private readonly AppDbContext _context;

    public PetService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PetResponseDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var pets = await _context.Pets
            .AsNoTracking()
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);

        return pets.Select(MapToResponse);
    }

    public async Task<PetResponseDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var pet = await _context.Pets
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return pet is null ? null : MapToResponse(pet);
    }

    public async Task<PetResponseDto> CriarAsync(PetCreateDto dto, CancellationToken cancellationToken = default)
    {
        var pet = new Pet
        {
            Nome = dto.Nome.Trim(),
            Raça = dto.Raça.Trim(),
            Cor = dto.Cor.Trim(),
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        _context.Pets.Add(pet);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponse(pet);
    }

    public async Task<PetResponseDto?> AtualizarAsync(int id, PetUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (pet is null)
        {
            return null;
        }

        pet.Nome = dto.Nome.Trim();
        pet.Raça = dto.Raça.Trim();
        pet.Cor = dto.Cor.Trim();
        pet.Ativo = dto.Ativo;
        pet.AtualizadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponse(pet);
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (pet is null)
        {
            return false;
        }

        _context.Pets.Remove(pet);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static PetResponseDto MapToResponse(Pet pet) => new()
    {
        Id = pet.Id,
        Nome = pet.Nome,
        Raça = pet.Raça,
        Cor = pet.Cor,
        Ativo = pet.Ativo,
        CriadoEm = pet.CriadoEm,
        AtualizadoEm = pet.AtualizadoEm
    };
}
