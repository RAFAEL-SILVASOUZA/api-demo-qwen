namespace ProdutosApi.Dtos;

/// <summary>
/// Dados para atualização de uma categoria existente.
/// </summary>
public class CategoriaUpdateDto
{
    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public bool Ativo { get; set; }
}
