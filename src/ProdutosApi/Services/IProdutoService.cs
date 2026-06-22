using ProdutosApi.Dtos;

namespace ProdutosApi.Services;

public interface IProdutoService
{
    Task<IEnumerable<ProdutoResponseDto>> ObterTodosAsync(CancellationToken cancellationToken = default);

    Task<ProdutoResponseDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ProdutoResponseDto> CriarAsync(ProdutoCreateDto dto, CancellationToken cancellationToken = default);

    Task<ProdutoResponseDto?> AtualizarAsync(int id, ProdutoUpdateDto dto, CancellationToken cancellationToken = default);

    Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default);
}
