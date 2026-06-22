using FluentValidation.TestHelper;
using ProdutosApi.Dtos;
using ProdutosApi.Validators;
using Xunit;

namespace ProdutosApi.Tests;

public class ProdutoCreateDtoValidatorTests
{
    private readonly ProdutoCreateDtoValidator _validator = new();

    private static ProdutoCreateDto Valido() => new()
    {
        Nome = "Produto Válido",
        Descricao = "Descrição",
        Preco = 10m,
        QuantidadeEmEstoque = 5
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
    public void DeveFalhar_QuandoNomeExcede100Caracteres()
    {
        var dto = Valido();
        dto.Nome = new string('a', 101);

        var resultado = _validator.TestValidate(dto);

        resultado.ShouldHaveValidationErrorFor(p => p.Nome);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-99.99)]
    public void DeveFalhar_QuandoPrecoMenorOuIgualAZero(decimal preco)
    {
        var dto = Valido();
        dto.Preco = preco;

        var resultado = _validator.TestValidate(dto);

        resultado.ShouldHaveValidationErrorFor(p => p.Preco);
    }

    [Fact]
    public void DeveFalhar_QuandoEstoqueNegativo()
    {
        var dto = Valido();
        dto.QuantidadeEmEstoque = -1;

        var resultado = _validator.TestValidate(dto);

        resultado.ShouldHaveValidationErrorFor(p => p.QuantidadeEmEstoque);
    }

    [Fact]
    public void DeveFalhar_QuandoDescricaoExcede500Caracteres()
    {
        var dto = Valido();
        dto.Descricao = new string('a', 501);

        var resultado = _validator.TestValidate(dto);

        resultado.ShouldHaveValidationErrorFor(p => p.Descricao);
    }
}
