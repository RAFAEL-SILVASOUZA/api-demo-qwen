namespace ProdutosApi.Dtos;

/// <summary>
/// Dados para atualização de um produto existente.
/// </summary>
public class ProdutoUpdateDto
{
    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public decimal Preco { get; set; }

    public int QuantidadeEmEstoque { get; set; }

    public bool Ativo { get; set; }
}
