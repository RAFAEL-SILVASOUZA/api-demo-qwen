using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProdutosApi.Dtos;
using ProdutosApi.Services;

namespace ProdutosApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoService _service;

    public ProdutosController(IProdutoService service)
    {
        _service = service;
    }

    /// <summary>Lista todos os produtos cadastrados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProdutoResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProdutoResponseDto>>> ObterTodos(CancellationToken cancellationToken)
    {
        var produtos = await _service.ObterTodosAsync(cancellationToken);
        return Ok(produtos);
    }

    /// <summary>Obtém um produto pelo seu identificador.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProdutoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProdutoResponseDto>> ObterPorId(int id, CancellationToken cancellationToken)
    {
        var produto = await _service.ObterPorIdAsync(id, cancellationToken);
        return produto is null ? NotFound() : Ok(produto);
    }

    /// <summary>Cadastra um novo produto.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProdutoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProdutoResponseDto>> Criar([FromBody] ProdutoCreateDto dto, CancellationToken cancellationToken)
    {
        var produto = await _service.CriarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = produto.Id }, produto);
    }

    /// <summary>Atualiza um produto existente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProdutoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProdutoResponseDto>> Atualizar(int id, [FromBody] ProdutoUpdateDto dto, CancellationToken cancellationToken)
    {
        var produto = await _service.AtualizarAsync(id, dto, cancellationToken);
        return produto is null ? NotFound() : Ok(produto);
    }

    /// <summary>Remove um produto.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int id, CancellationToken cancellationToken)
    {
        var removido = await _service.RemoverAsync(id, cancellationToken);
        return removido ? NoContent() : NotFound();
    }
}
