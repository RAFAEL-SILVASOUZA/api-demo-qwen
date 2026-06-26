using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using ProdutosApi.Data;
using ProdutosApi.Dtos;
using ProdutosApi.Models;

namespace ProdutosApi.Tests.IntegrationTests;

public class AuthControllerIntegrationTests : IClassFixture<TestApplicationFactory>, IAsyncLifetime
{
    private readonly TestApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthControllerIntegrationTests(TestApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await using var context = _factory.Services.GetRequiredService<AppDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await using var context = _factory.Services.GetRequiredService<AppDbContext>();
        await context.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task PostRegistrar_DeveRetornar201_ePersistirUsuarioNoPostgres()
    {
        var dto = new UserCreateDto
        {
            Name = "João Silva",
            Email = "joao@email.com",
            Senha = "123456"
        };

        var response = await _client.PostAsJsonAsync("api/auth/registrar", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var resultado = await response.Content.ReadFromJsonAsync<UserResponseDto>();

        resultado.Should().NotBeNull();
        resultado!.Id.Should().BePositive();
        resultado.Name.Should().Be("João Silva");
        resultado.Email.Should().Be("joao@email.com");
        resultado.IsAdmin.Should().BeFalse();
        resultado.Ativo.Should().BeTrue();

        await using var context = _factory.Services.GetRequiredService<AppDbContext>();
        (await ((IQueryable<User>)context.Users).CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task PostAutenticar_DeveRetornar200_eToken_QuandoCredenciaisValidas()
    {
        await using var context = _factory.Services.GetRequiredService<AppDbContext>();
        context.Users.Add(new User
        {
            Name = "João Silva",
            Email = "joao@email.com",
            PasswordHash = "hash",
            Salt = new byte[] { 1, 2, 3 },
            IsAdmin = false,
            Ativo = true
        });
        await context.SaveChangesAsync();

        var dto = new UserLoginDto
        {
            Email = "joao@email.com",
            Senha = "123456"
        };

        var response = await _client.PostAsJsonAsync("api/auth/autenticar", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<UserResponseDto>();

        resultado.Should().NotBeNull();
        resultado!.Id.Should().BePositive();
    }

    [Fact]
    public async Task PostAutenticar_DeveRetornar401_QuandoSenhaIncorreta()
    {
        await using var context = _factory.Services.GetRequiredService<AppDbContext>();
        context.Users.Add(new User
        {
            Name = "João Silva",
            Email = "joao@email.com",
            PasswordHash = "hash",
            Salt = new byte[] { 1, 2, 3 },
            IsAdmin = false,
            Ativo = true
        });
        await context.SaveChangesAsync();

        var dto = new UserLoginDto
        {
            Email = "joao@email.com",
            Senha = "senhaErrada"
        };

        var response = await _client.PostAsJsonAsync("api/auth/autenticar", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostAutenticar_DeveRetornar401_QuandoEmailNaoExiste()
    {
        var dto = new UserLoginDto
        {
            Email = "naoexiste@email.com",
            Senha = "123456"
        };

        var response = await _client.PostAsJsonAsync("api/auth/autenticar", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
