namespace ProdutosApi.Models;

/// <summary>
/// Entidade de domínio que representa uma categoria de produto.
/// </summary>
public class Categoria
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public DateTime? AtualizadoEm { get; set; }

    public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}
