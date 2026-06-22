using Microsoft.AspNetCore.Mvc;
using ProdutosApi.Dtos;
using ProdutosApi.Services;

namespace ProdutosApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaService _service;

    public CategoriasController(ICategoriaService service)
    {
        _service = service;
    }

    /// <summary>Lista todas as categorias cadastradas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoriaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CategoriaResponseDto>>> ObterTodos(CancellationToken cancellationToken)
    {
        var categorias = await _service.ObterTodosAsync(cancellationToken);
        return Ok(categorias);
    }

    /// <summary>Obtém uma categoria pelo seu identificador.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CategoriaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoriaResponseDto>> ObterPorId(int id, CancellationToken cancellationToken)
    {
        var categoria = await _service.ObterPorIdAsync(id, cancellationToken);
        return categoria is null ? NotFound() : Ok(categoria);
    }

    /// <summary>Cadastra uma nova categoria.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CategoriaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoriaResponseDto>> Criar([FromBody] CategoriaCreateDto dto, CancellationToken cancellationToken)
    {
        var categoria = await _service.CriarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = categoria.Id }, categoria);
    }

    /// <summary>Atualiza uma categoria existente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CategoriaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoriaResponseDto>> Atualizar(int id, [FromBody] CategoriaUpdateDto dto, CancellationToken cancellationToken)
    {
        var categoria = await _service.AtualizarAsync(id, dto, cancellationToken);
        return categoria is null ? NotFound() : Ok(categoria);
    }

    /// <summary>Remove uma categoria.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int id, CancellationToken cancellationToken)
    {
        var removido = await _service.RemoverAsync(id, cancellationToken);
        return removido ? NoContent() : NotFound();
    }
}
