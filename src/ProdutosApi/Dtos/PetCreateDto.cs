namespace ProdutosApi.Dtos;

/// <summary>
/// Dados necessários para cadastrar um novo animal de estimação.
/// </summary>
public class PetCreateDto
{
    public string Nome { get; set; } = string.Empty;

    public string Raça { get; set; } = string.Empty;

    public string Cor { get; set; } = string.Empty;
}
