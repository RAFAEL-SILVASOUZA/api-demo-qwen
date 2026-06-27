namespace ProdutosApi.Dtos;

/// <summary>
/// Representação de um animal de estimação retornado pela API.
/// </summary>
public class PetResponseDto
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Raça { get; set; } = string.Empty;

    public string Cor { get; set; } = string.Empty;

    public bool Ativo { get; set; }

    public DateTime CriadoEm { get; set; }

    public DateTime? AtualizadoEm { get; set; }
}
