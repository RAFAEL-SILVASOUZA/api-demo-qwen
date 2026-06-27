namespace ProdutosApi.Dtos;

/// <summary>
/// Dados para atualização de um animal de estimação existente.
/// </summary>
public class PetUpdateDto
{
    public string Nome { get; set; } = string.Empty;

    public string Raça { get; set; } = string.Empty;

    public string Cor { get; set; } = string.Empty;

    public bool Ativo { get; set; }
}
