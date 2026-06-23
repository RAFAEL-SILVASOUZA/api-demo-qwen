using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ProdutosApi.Data;
using ProdutosApi.Dtos;
using ProdutosApi.Models;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace ProdutosApi.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<(UserResponseDto? User, string Token)> RegistrarAsync(UserCreateDto dto, CancellationToken cancellationToken = default)
    {
        var validator = new Validators.UserCreateDtoValidator();
        var result = await validator.ValidateAsync(dto, cancellationToken);

        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors.First().ErrorMessage);
        }

        var existe = await _context.Users.AnyAsync(u => u.Email == dto.Email.Trim(), cancellationToken);
        if (existe)
        {
            throw new InvalidOperationException("E-mail já cadastrado.");
        }

        var salt = GerarSalt();
        var passwordHash = HashSenha(dto.Senha, salt);

        var user = new User
        {
            Name = dto.Name.Trim(),
            Email = dto.Email.Trim(),
            PasswordHash = passwordHash,
            Salt = salt,
            IsAdmin = dto.IsAdmin,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        var token = GerarToken(user.Id, user.Email);

        return (MapToResponse(user), token);
    }

    public async Task<(UserResponseDto? User, string Token)> AutenticarAsync(UserLoginDto dto, CancellationToken cancellationToken = default)
    {
        var validator = new Validators.UserLoginDtoValidator();
        var result = await validator.ValidateAsync(dto, cancellationToken);

        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors.First().ErrorMessage);
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == dto.Email.Trim(), cancellationToken);

        if (user == null || !VerificarSenha(dto.Senha, user.PasswordHash, user.Salt))
        {
            return (null, string.Empty);
        }

        if (!user.Ativo)
        {
            return (null, string.Empty);
        }

        var token = GerarToken(user.Id, user.Email);

        return (MapToResponse(user), token);
    }

    public async Task<bool> UsuarioExisteAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(u => u.Email == email.Trim(), cancellationToken);
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

    private static bool VerificarSenha(string senha, string hashArmazenado, byte[] salt)
    {
        return HashSenha(senha, salt) == hashArmazenado;
    }

    private string GerarToken(int userId, string email)
    {
        var jwtSettings = _configuration.GetSection("jwt");
        var secretKey = jwtSettings["secretKey"] ?? throw new InvalidOperationException("Secret key não configurada em appsettings.json");
        var issuer = jwtSettings["issuer"] ?? "ProdutosApi";
        var audience = jwtSettings["audience"] ?? "http://localhost:5000";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, email),
            new System.Security.Claims.Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserResponseDto MapToResponse(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            IsAdmin = user.IsAdmin,
            Ativo = user.Ativo,
            CriadoEm = user.CriadoEm
        };
    }
}
