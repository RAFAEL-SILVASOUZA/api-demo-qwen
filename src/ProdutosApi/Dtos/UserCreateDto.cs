namespace ProdutosApi.Dtos;

/// <summary>
/// Dados necessários para cadastrar um novo usuário.
/// </summary>
public class UserCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
}
