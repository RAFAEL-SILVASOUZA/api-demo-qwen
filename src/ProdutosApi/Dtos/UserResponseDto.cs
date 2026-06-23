namespace ProdutosApi.Dtos;

/// <summary>
/// Representação de um usuário retornada pela API.
/// </summary>
public class UserResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
}
