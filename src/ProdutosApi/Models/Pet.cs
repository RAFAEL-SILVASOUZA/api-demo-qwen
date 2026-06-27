namespace ProdutosApi.Models;

/// <summary>
/// Entidade de domínio que representa um animal de estimação cadastrado.
/// </summary>
public class Pet
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Raça { get; set; } = string.Empty;

    public string Cor { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public DateTime? AtualizadoEm { get; set; }
}
