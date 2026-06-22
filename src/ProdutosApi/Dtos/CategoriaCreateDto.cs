namespace ProdutosApi.Dtos;

/// <summary>
/// Dados necessários para cadastrar uma nova categoria.
/// </summary>
public class CategoriaCreateDto
{
    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }
}
