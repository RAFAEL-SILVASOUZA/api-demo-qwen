using Microsoft.AspNetCore.Mvc;
using ProdutosApi.Dtos;
using ProdutosApi.Services;

namespace ProdutosApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registrar([FromBody] UserCreateDto dto, CancellationToken ct)
    {
        if (dto == null)
            return BadRequest("Dados inválidos.");

        var existe = await _authService.UsuarioExisteAsync(dto.Email, ct);
        if (existe)
            return BadRequest("E-mail já cadastrado.");

        try
        {
            var resultado = await _authService.RegistrarAsync(dto, ct);
            return CreatedAtAction(nameof(Autenticar), new { }, new { Token = resultado.Token });
        }
        catch (Exception ex) when (ex is FluentValidation.ValidationException || ex is InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Autenticar([FromBody] UserLoginDto dto, CancellationToken ct)
    {
        if (dto == null)
            return BadRequest("Dados inválidos.");

        try
        {
            var resultado = await _authService.AutenticarAsync(dto, ct);
            if (resultado.User == null)
                return Unauthorized("E-mail ou senha inválidos.");

            return Ok(new { Token = resultado.Token });
        }
        catch (Exception ex) when (ex is FluentValidation.ValidationException)
        {
            return BadRequest(ex.Message);
        }
    }
}
