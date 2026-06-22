using ProdutosApi.Dtos;

namespace ProdutosApi.Services;

public interface ICategoriaService
{
    Task<IEnumerable<CategoriaResponseDto>> ObterTodosAsync(CancellationToken cancellationToken = default);

    Task<CategoriaResponseDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<CategoriaResponseDto> CriarAsync(CategoriaCreateDto dto, CancellationToken cancellationToken = default);

    Task<CategoriaResponseDto?> AtualizarAsync(int id, CategoriaUpdateDto dto, CancellationToken cancellationToken = default);

    Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default);
}
