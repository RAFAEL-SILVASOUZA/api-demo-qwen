using FluentValidation.TestHelper;
using ProdutosApi.Dtos;
using ProdutosApi.Validators;
using Xunit;

namespace ProdutosApi.Tests;

public class CategoriaCreateDtoValidatorTests
{
    private readonly CategoriaCreateDtoValidator _validator = new();

    private static CategoriaCreateDto Valido() => new()
    {
        Nome = "Categoria Válida",
        Descricao = "Descrição"
    };

    [Fact]
    public void DeveSerValido_QuandoTodosOsCamposEstaoCorretos()
    {
        var resultado = _validator.TestValidate(Valido());
        resultado.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void DeveFalhar_QuandoNomeVazio(string nome)
    {
        var dto = Valido();
        dto.Nome = nome;

        var resultado = _validator.TestValidate(dto);

        resultado.ShouldHaveValidationErrorFor(c => c.Nome);
    }

    [Fact]
    public void DeveFalhar_QuandoNomeExcede80Caracteres()
    {
        var dto = Valido();
        dto.Nome = new string('a', 81);

        var resultado = _validator.TestValidate(dto);

        resultado.ShouldHaveValidationErrorFor(c => c.Nome);
    }

    [Fact]
    public void DeveFalhar_QuandoDescricaoExcede300Caracteres()
    {
        var dto = Valido();
        dto.Descricao = new string('a', 301);

        var resultado = _validator.TestValidate(dto);

        resultado.ShouldHaveValidationErrorFor(c => c.Descricao);
    }
}
