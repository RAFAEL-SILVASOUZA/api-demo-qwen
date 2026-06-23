using FluentAssertions;
using ProdutosApi.Dtos;
using ProdutosApi.Validators;

namespace ProdutosApi.Tests;

public class UserLoginDtoValidatorTests
{
    private readonly UserLoginDtoValidator _validator = new();

    [Fact]
    public void Validacao_DeveFalhar_QuandoEmailVazio()
    {
        // Arrange
        var dto = new UserLoginDto
        {
            Email = "",
            Senha = "123456"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count(e => e.PropertyName == "Email").Should().BeGreaterThan(0);
    }

    [Fact]
    public void Validacao_DeveFalhar_QuandoEmailInvalido()
    {
        // Arrange
        var dto = new UserLoginDto
        {
            Email = "email-invalido",
            Senha = "123456"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count(e => e.PropertyName == "Email").Should().BeGreaterThan(0);
    }

    [Fact]
    public void Validacao_DeveFalhar_QuandoSenhaVazia()
    {
        // Arrange
        var dto = new UserLoginDto
        {
            Email = "joao@email.com",
            Senha = ""
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count(e => e.PropertyName == "Senha").Should().BeGreaterThan(0);
    }

    [Fact]
    public void Validacao_DevePassar_QuandoDadosValidos()
    {
        // Arrange
        var dto = new UserLoginDto
        {
            Email = "joao@email.com",
            Senha = "123456"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
