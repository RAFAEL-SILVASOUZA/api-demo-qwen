using FluentValidation.TestHelper;
using ProdutosApi.Dtos;
using ProdutosApi.Validators;
using Xunit;

namespace ProdutosApi.Tests;

public class PetCreateDtoValidatorTests
{
    private readonly PetCreateDtoValidator _validator = new();

    private static PetCreateDto Valido() => new()
    {
        Nome = "Rex",
        Raça = "Labrador",
        Cor = "Dourado"
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

        resultado.ShouldHaveValidationErrorFor(p => p.Nome);
    }

    [Fact]
    public void DeveFalhar_QuandoNomeExcede80Caracteres()
    {
        var dto = Valido();
        dto.Nome = new string('a', 81);

        var resultado = _validator.TestValidate(dto);

        resultado.ShouldHaveValidationErrorFor(p => p.Nome);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void DeveFalhar_QuandoRacaVazia(string raca)
    {
        var dto = Valido();
        dto.Raça = raca;

        var resultado = _validator.TestValidate(dto);

        resultado.ShouldHaveValidationErrorFor(p => p.Raça);
    }

    [Fact]
    public void DeveFalhar_QuandoRacaExcede80Caracteres()
    {
        var dto = Valido();
        dto.Raça = new string('b', 81);

        var resultado = _validator.TestValidate(dto);

        resultado.ShouldHaveValidationErrorFor(p => p.Raça);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void DeveFalhar_QuandoCorVazia(string cor)
    {
        var dto = Valido();
        dto.Cor = cor;

        var resultado = _validator.TestValidate(dto);

        resultado.ShouldHaveValidationErrorFor(p => p.Cor);
    }

    [Fact]
    public void DeveFalhar_QuandoCorExcede50Caracteres()
    {
        var dto = Valido();
        dto.Cor = new string('c', 51);

        var resultado = _validator.TestValidate(dto);

        resultado.ShouldHaveValidationErrorFor(p => p.Cor);
    }
}
