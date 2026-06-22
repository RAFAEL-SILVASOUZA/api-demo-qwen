namespace ProdutosApi.Dtos;

/// <summary>
/// Representação de uma categoria retornada pela API.
/// </summary>
public class CategoriaResponseDto
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public bool Ativo { get; set; }

    public DateTime CriadoEm { get; set; }

    public DateTime? AtualizadoEm { get; set; }
}
