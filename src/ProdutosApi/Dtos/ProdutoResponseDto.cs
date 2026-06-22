namespace ProdutosApi.Dtos;

/// <summary>
/// Representação de um produto retornado pela API.
/// </summary>
public class ProdutoResponseDto
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public decimal Preco { get; set; }

    public int QuantidadeEmEstoque { get; set; }

    public bool Ativo { get; set; }

    public DateTime CriadoEm { get; set; }

    public DateTime? AtualizadoEm { get; set; }
}
