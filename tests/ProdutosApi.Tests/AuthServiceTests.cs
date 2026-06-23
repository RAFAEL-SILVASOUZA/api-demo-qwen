using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProdutosApi.Data;
using ProdutosApi.Dtos;
using ProdutosApi.Services;

namespace ProdutosApi.Tests;

public class AuthServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("AuthTestDb_" + Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static IConfiguration CreateConfiguration()
    {
        var configData = new Dictionary<string, string>
        {
            { "jwt:secretKey", "MinhaChaveSecretaJWT2024!@#$MinhaChaveSecretaJWT2024!@#$" },
            { "jwt:issuer", "ProdutosApi" },
            { "jwt:audience", "http://localhost:5000" }
        };
        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    [Fact]
    public async Task RegistrarAsync_DeveCriarUsuarioEGerarToken()
    {
        // Arrange
        var context = CreateContext();
        await context.Database.EnsureCreatedAsync();
        var config = CreateConfiguration();
        var service = new AuthService(context, config);

        var dto = new UserCreateDto
        {
            Name = "João Silva",
            Email = "joao@email.com",
            Senha = "123456",
            IsAdmin = false
        };

        // Act
        var resultado = await service.RegistrarAsync(dto);

        // Assert
        resultado.User.Should().NotBeNull();
        resultado.User!.Id.Should().BeGreaterThan(0);
        resultado.User.Name.Should().Be("João Silva");
        resultado.User.Email.Should().Be("joao@email.com");
        resultado.User.IsAdmin.Should().BeFalse();
        resultado.User.Ativo.Should().BeTrue();
        resultado.Token.Should().NotBeNullOrEmpty();

        var userNoDb = await context.Users.FindAsync([resultado.User.Id]);
        userNoDb.Should().NotBeNull();
        userNoDb!.PasswordHash.Should().NotBeNullOrEmpty();
        userNoDb.Salt.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RegistrarAsync_DeveFalhar_QuandoEmailDuplicado()
    {
        // Arrange
        var context = CreateContext();
        await context.Database.EnsureCreatedAsync();
        var config = CreateConfiguration();
        var service = new AuthService(context, config);

        var dto = new UserCreateDto
        {
            Name = "João Silva",
            Email = "joao@email.com",
            Senha = "123456",
            IsAdmin = false
        };

        await service.RegistrarAsync(dto);

        // Act
        var segundoDto = new UserCreateDto
        {
            Name = "Outro Usuário",
            Email = "joao@email.com",
            Senha = "123456",
            IsAdmin = false
        };

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.RegistrarAsync(segundoDto));
    }

    [Fact]
    public async Task AutenticarAsync_DeveRetornarToken_QuandoCredenciaisValidas()
    {
        // Arrange
        var context = CreateContext();
        await context.Database.EnsureCreatedAsync();
        var config = CreateConfiguration();
        var service = new AuthService(context, config);

        var dtoRegistro = new UserCreateDto
        {
            Name = "João Silva",
            Email = "joao@email.com",
            Senha = "123456",
            IsAdmin = false
        };

        await service.RegistrarAsync(dtoRegistro);

        var dtoLogin = new UserLoginDto
        {
            Email = "joao@email.com",
            Senha = "123456"
        };

        // Act
        var resultado = await service.AutenticarAsync(dtoLogin);

        // Assert
        resultado.User.Should().NotBeNull();
        resultado.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AutenticarAsync_DeveRetornarNull_QuandoSenhaIncorreta()
    {
        // Arrange
        var context = CreateContext();
        await context.Database.EnsureCreatedAsync();
        var config = CreateConfiguration();
        var service = new AuthService(context, config);

        var dtoRegistro = new UserCreateDto
        {
            Name = "João Silva",
            Email = "joao@email.com",
            Senha = "123456",
            IsAdmin = false
        };

        await service.RegistrarAsync(dtoRegistro);

        var dtoLogin = new UserLoginDto
        {
            Email = "joao@email.com",
            Senha = "senhaErrada"
        };

        // Act
        var resultado = await service.AutenticarAsync(dtoLogin);

        // Assert
        resultado.User.Should().BeNull();
        resultado.Token.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task AutenticarAsync_DeveRetornarNull_QuandoEmailNaoExiste()
    {
        // Arrange
        var context = CreateContext();
        await context.Database.EnsureCreatedAsync();
        var config = CreateConfiguration();
        var service = new AuthService(context, config);

        var dtoLogin = new UserLoginDto
        {
            Email = "naoexiste@email.com",
            Senha = "123456"
        };

        // Act
        var resultado = await service.AutenticarAsync(dtoLogin);

        // Assert
        resultado.User.Should().BeNull();
    }

    [Fact]
    public async Task UsuarioExisteAsync_DeveRetornarTrue_QuandoEmailCadastrado()
    {
        // Arrange
        var context = CreateContext();
        await context.Database.EnsureCreatedAsync();
        var config = CreateConfiguration();
        var service = new AuthService(context, config);

        var dto = new UserCreateDto
        {
            Name = "João Silva",
            Email = "joao@email.com",
            Senha = "123456",
            IsAdmin = false
        };

        await service.RegistrarAsync(dto);

        // Act
        var existe = await service.UsuarioExisteAsync("joao@email.com");

        // Assert
        existe.Should().BeTrue();
    }

    [Fact]
    public async Task UsuarioExisteAsync_DeveRetornarFalse_QuandoEmailNaoCadastrado()
    {
        // Arrange
        var context = CreateContext();
        await context.Database.EnsureCreatedAsync();
        var config = CreateConfiguration();
        var service = new AuthService(context, config);

        // Act
        var existe = await service.UsuarioExisteAsync("naoexiste@email.com");

        // Assert
        existe.Should().BeFalse();
    }
}
