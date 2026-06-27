using ProdutosApi.Dtos;

namespace ProdutosApi.Services;

public interface IPetService
{
    Task<IEnumerable<PetResponseDto>> ObterTodosAsync(CancellationToken cancellationToken = default);

    Task<PetResponseDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PetResponseDto> CriarAsync(PetCreateDto dto, CancellationToken cancellationToken = default);

    Task<PetResponseDto?> AtualizarAsync(int id, PetUpdateDto dto, CancellationToken cancellationToken = default);

    Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default);
}
