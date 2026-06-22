namespace ProdutosApi.Dtos;

/// <summary>
/// Dados necessários para cadastrar um novo produto.
/// </summary>
public class ProdutoCreateDto
{
    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public decimal Preco { get; set; }

    public int QuantidadeEmEstoque { get; set; }

    public int? CategoriaId { get; set; }
}
