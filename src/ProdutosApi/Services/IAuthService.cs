using ProdutosApi.Dtos;
using ProdutosApi.Models;

namespace ProdutosApi.Services;

public interface IAuthService
{
    Task<(UserResponseDto? User, string Token)> RegistrarAsync(UserCreateDto dto, CancellationToken cancellationToken = default);
    Task<(UserResponseDto? User, string Token)> AutenticarAsync(UserLoginDto dto, CancellationToken cancellationToken = default);
    Task<bool> UsuarioExisteAsync(string email, CancellationToken cancellationToken = default);
}
