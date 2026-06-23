using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using ProdutosApi.Data;
using ProdutosApi.Dtos;

namespace ProdutosApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterTodos(CancellationToken ct)
    {
        var usuarios = await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                IsAdmin = u.IsAdmin,
                Ativo = u.Ativo,
                CriadoEm = u.CriadoEm
            })
            .ToListAsync(ct);

        return Ok(usuarios);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
    {
        var usuario = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                IsAdmin = u.IsAdmin,
                Ativo = u.Ativo,
                CriadoEm = u.CriadoEm
            })
            .FirstOrDefaultAsync(ct);

        if (usuario == null)
            return NotFound("Usuário não encontrado.");

        return Ok(usuario);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] UserCreateDto dto, CancellationToken ct)
    {
        var usuario = await _context.Users.FindAsync([id], ct);
        if (usuario == null)
            return NotFound("Usuário não encontrado.");

        usuario.Name = dto.Name.Trim();
        usuario.Email = dto.Email.Trim();
        usuario.IsAdmin = dto.IsAdmin;
        usuario.AtualizadoEm = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(dto.Senha))
        {
            usuario.Salt = GerarSalt();
            usuario.PasswordHash = HashSenha(dto.Senha, usuario.Salt);
        }

        await _context.SaveChangesAsync(ct);

        return Ok(new UserResponseDto
        {
            Id = usuario.Id,
            Name = usuario.Name,
            Email = usuario.Email,
            IsAdmin = usuario.IsAdmin,
            Ativo = usuario.Ativo,
            CriadoEm = usuario.CriadoEm
        });
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Remover(int id, CancellationToken ct)
    {
        var usuario = await _context.Users.FindAsync([id], ct);
        if (usuario == null)
            return NotFound("Usuário não encontrado.");

        _context.Users.Remove(usuario);
        await _context.SaveChangesAsync(ct);

        return NoContent();
    }

    private static byte[] GerarSalt()
    {
        var salt = new byte[128 / 8];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return salt;
    }

    private static string HashSenha(string senha, byte[] salt)
    {
        return Convert.ToBase64String(KeyDerivation.Pbkdf2(
            senha, salt, KeyDerivationPrf.HMACSHA1, 10000, 256 / 8));
    }
}
