using FluentAssertions;
using ProdutosApi.Dtos;
using ProdutosApi.Validators;

namespace ProdutosApi.Tests;

public class UserCreateDtoValidatorTests
{
    private readonly UserCreateDtoValidator _validator = new();

    [Fact]
    public void Validacao_DeveFalhar_QuandoNomeVazio()
    {
        // Arrange
        var dto = new UserCreateDto
        {
            Name = "",
            Email = "joao@email.com",
            Senha = "123456"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count(e => e.PropertyName == "Name").Should().BeGreaterThan(0);
    }

    [Fact]
    public void Validacao_DeveFalhar_QuandoNomeMaiorQue50Caracteres()
    {
        // Arrange
        var dto = new UserCreateDto
        {
            Name = new string('a', 51),
            Email = "joao@email.com",
            Senha = "123456"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count(e => e.PropertyName == "Name").Should().BeGreaterThan(0);
    }

    [Fact]
    public void Validacao_DeveFalhar_QuandoEmailInvalido()
    {
        // Arrange
        var dto = new UserCreateDto
        {
            Name = "João Silva",
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
    public void Validacao_DeveFalhar_QuandoEmailVazio()
    {
        // Arrange
        var dto = new UserCreateDto
        {
            Name = "João Silva",
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
    public void Validacao_DeveFalhar_QuandoSenhaMenorQue6Caracteres()
    {
        // Arrange
        var dto = new UserCreateDto
        {
            Name = "João Silva",
            Email = "joao@email.com",
            Senha = "12345"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count(e => e.PropertyName == "Senha").Should().BeGreaterThan(0);
    }

    [Fact]
    public void Validacao_DeveFalhar_QuandoSenhaVazia()
    {
        // Arrange
        var dto = new UserCreateDto
        {
            Name = "João Silva",
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
        var dto = new UserCreateDto
        {
            Name = "João Silva",
            Email = "joao@email.com",
            Senha = "123456"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
