using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProdutosApi.Dtos;
using ProdutosApi.Services;

namespace ProdutosApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class PetsController : ControllerBase
{
    private readonly IPetService _service;

    public PetsController(IPetService service)
    {
        _service = service;
    }

    /// <summary>Lista todos os animais de estimação cadastrados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PetResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PetResponseDto>>> ObterTodos(CancellationToken cancellationToken)
    {
        var pets = await _service.ObterTodosAsync(cancellationToken);
        return Ok(pets);
    }

    /// <summary>Obtém um animal de estimação pelo seu identificador.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PetResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PetResponseDto>> ObterPorId(int id, CancellationToken cancellationToken)
    {
        var pet = await _service.ObterPorIdAsync(id, cancellationToken);
        return pet is null ? NotFound() : Ok(pet);
    }

    /// <summary>Cadastra um novo animal de estimação.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PetResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PetResponseDto>> Criar([FromBody] PetCreateDto dto, CancellationToken cancellationToken)
    {
        var pet = await _service.CriarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = pet.Id }, pet);
    }

    /// <summary>Atualiza um animal de estimação existente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(PetResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PetResponseDto>> Atualizar(int id, [FromBody] PetUpdateDto dto, CancellationToken cancellationToken)
    {
        var pet = await _service.AtualizarAsync(id, dto, cancellationToken);
        return pet is null ? NotFound() : Ok(pet);
    }

    /// <summary>Remove um animal de estimação.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int id, CancellationToken cancellationToken)
    {
        var removido = await _service.RemoverAsync(id, cancellationToken);
        return removido ? NoContent() : NotFound();
    }
}
