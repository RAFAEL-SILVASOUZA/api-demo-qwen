---
name: jwt-auth
description: Implementa autenticação JWT completa no projeto ProdutosApi seguindo Clean Architecture (referência: ribeirodavi04/autenticacao-jwt). Cria entidade User, DTOs, Validators, serviço de autenticação com geração/validação de token JWT, hash de senha com PBKDF2, controladores de Auth e Users protegidos, e registra tudo no Program.cs.
---

# Skill: Autenticação JWT (.NET 8 — Clean Architecture)

Você é um agente especializado em **autenticação JWT** para APIs ASP.NET Core.
Sua tarefa é implementar autenticação completa no projeto **ProdutosApi**,
seguindo o padrão do repositório de referência
[ribeirodavi04/autenticacao-jwt](https://github.com/ribeirodavi04/autenticacao-jwt)
adaptado às convenções existentes do projeto (nomes em português, FluentValidation,
async/CancellationToken, banco InMemory).

## Referência principal

O repositório de referência implementa JWT com:
- **5 camadas**: API → Application → Domain → Infrastructure → IoC
- **Entidade User** com `Id`, `Name`, `Email`, `Password`, `Salt`, `IsAdmin`
- **Hash de senha** via `KeyDerivation.Pbkdf2` (HMACSHA1, 10000 iterações)
- **Geração de token JWT** com `JwtSecurityTokenHandler`, `SymmetricSecurityKey`, claims de `id` e `email`
- **Controller Login** com endpoints `/api/Login/register` e `/api/Login/login`
- **Controller User** protegido com `[Authorize]`, com CRUD completo
- **Configuração JWT** em `appsettings.json` (`secretKey`, `issuer`, `audience`)

## Convenções do projeto ProdutosApi (NÃO desvie)

Antes de escrever qualquer código, **leia os arquivos de referência** para copiar
o estilo exato:

- `src/ProdutosApi/Models/Produto.cs`
- `src/ProdutosApi/Dtos/ProdutoCreateDto.cs`, `ProdutoUpdateDto.cs`, `ProdutoResponseDto.cs`
- `src/ProdutosApi/Validators/ProdutoCreateDtoValidator.cs`
- `src/ProdutosApi/Services/IProdutoService.cs`, `ProdutoService.cs`
- `src/ProdutosApi/Controllers/ProdutosController.cs`
- `src/ProdutosApi/Data/AppDbContext.cs`
- `src/ProdutosApi/Program.cs`
- `tests/ProdutosApi.Tests/ProdutoServiceTests.cs`

Regras de estilo derivadas:

1. **Idioma**: tudo em português (nomes de métodos: `ObterTodosAsync`, `AutenticarAsync`,
   `GerarToken`, `CriarAsync`). Propriedades e mensagens em português.
2. **Entidade** (`Models/`): sempre tem `int Id`, `bool Ativo` (default `true`),
   `DateTime CriadoEm = DateTime.UtcNow`, `DateTime? AtualizadoEm`.
3. **DTOs** (`Dtos/`): três arquivos — `XCreateDto`, `XUpdateDto`, `XResponseDto`.
4. **Validators** (`Validators/`): `AbstractValidator<T>`, mensagens em português.
5. **Service** (`Services/`): interface + implementação, `CancellationToken cancellationToken = default`,
   mapeamento manual DTO↔entidade.
6. **Controller** (`Controllers/`): `[ApiController]`, `[Route("api/[controller]")]`,
   `[Produces("application/json")]`, `<summary>` em cada action, `[ProducesResponseType]`.
7. **DbContext**: `DbSet<X> Xs => Set<X>();`, configurações em `OnModelCreating`.
8. **Testes** (`tests/`): xUnit + FluentAssertions, banco InMemory isolado.

## Arquitetura a ser implementada

```
src/ProdutosApi/
├── Models/
│   └── User.cs                          ← Entidade User (Id, Name, Email, PasswordHash, Salt, IsAdmin, Ativo, CriadoEm, AtualizadoEm)
├── Dtos/
│   ├── UserCreateDto.cs                 ← Nome, Email, Senha, IsAdmin (opcional)
│   ├── UserLoginDto.cs                  ← Email, Senha
│   └── UserResponseDto.cs              ← Id, Name, Email, IsAdmin, Ativo, CriadoEm
├── Validators/
│   ├── UserCreateDtoValidator.cs        ← NotEmpty nome/email/senha, EmailAddress, MinLength 6 na senha
│   └── UserLoginDtoValidator.cs         ← NotEmpty email/senha, EmailAddress
├── Services/
│   ├── IAuthService.cs                  ← AutenticarAsync, RegistrarAsync, GerarToken, UsuarioExisteAsync
│   └── AuthService.cs                   ← Implementação com hash PBKDF2 + geração JWT
├── Controllers/
│   ├── AuthController.cs                ← POST /api/Auth/register, POST /api/Auth/login (sem [Authorize])
│   └── UsersController.cs              ← GET, GET/{id}, PUT, DELETE/{id} com [Authorize]
├── Data/
│   └── AppDbContext.cs                  ← Adicionar DbSet<User> + configuração
├── appsettings.json                     ← Adicionar seção "jwt" com secretKey, issuer, audience
└── Program.cs                           ← Registrar JWT bearer auth, DI services, using

tests/ProdutosApi.Tests/
├── AuthServiceTests.cs                  ← Testes de autenticação (register, login, token, senha incorreta)
├── UserCreateDtoValidatorTests.cs       ← Validação do DTO de criação
└── UserLoginDtoValidatorTests.cs        ← Validação do DTO de login
```

## Procedimento detalhado

### 1. Instalar pacotes NuGet necessários

Adicionar ao `src/ProdutosApi/ProdutosApi.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.15.0" />
</ItemGroup>
```

### 2. Criar a entidade User (`Models/User.cs`)

```csharp
namespace ProdutosApi.Models;

public class User
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public byte[] Salt { get; set; } = [];

    public bool IsAdmin { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public DateTime? AtualizadoEm { get; set; }
}
```

**Diferença da referência**: usa `PasswordHash` (em vez de `Password`) e segue o padrão
de datas/ativo do projeto existente.

### 3. Criar DTOs

**`Dtos/UserCreateDto.cs`**:
```csharp
namespace ProdutosApi.Dtos;

public class UserCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
}
```

**`Dtos/UserLoginDto.cs`**:
```csharp
namespace ProdutosApi.Dtos;

public class UserLoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}
```

**`Dtos/UserResponseDto.cs`**:
```csharp
namespace ProdutosApi.Dtos;

public class UserResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
}
```

### 4. Criar Validators

**`Validators/UserCreateDtoValidator.cs`**:
- `Name`: `NotEmpty()`, `MaximumLength(50)`
- `Email`: `NotEmpty()`, `EmailAddress()`
- `Senha`: `NotEmpty()`, `MinimumLength(6)`

**`Validators/UserLoginDtoValidator.cs`**:
- `Email`: `NotEmpty()`, `EmailAddress()`
- `Senha`: `NotEmpty()`

### 5. Criar a interface e implementação do AuthService

**`Services/IAuthService.cs`**:
```csharp
namespace ProdutosApi.Services;

public interface IAuthService
{
    Task<(UserResponseDto User, string Token)> RegistrarAsync(UserCreateDto dto, CancellationToken cancellationToken = default);
    Task<(UserResponseDto User, string Token)> AutenticarAsync(UserLoginDto dto, CancellationToken cancellationToken = default);
    Task<bool> UsuarioExisteAsync(string email, CancellationToken cancellationToken = default);
    Task<UserResponseDto?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);
}
```

**`Services/AuthService.cs`**:

Implementação completa com:
- **Injeção de dependência**: `AppDbContext` (e `IConfiguration` para JWT settings)
- **Hash de senha**: usar `KeyDerivation.Pbkdf2` do .NET (mesma técnica da referência):
  ```csharp
  private static byte[] GerarSalt()
  {
      var salt = new byte[128 / 8];
      using var rng = RandomNumberGenerator.Create();
      rng.GetBytes(salt);
      return salt;
  }

  private static string HashSenha(string senha, byte[] salt)
  {
      return Convert.ToBase64String(KeyDerivation.Pbkdf2(
          senha, salt, KeyDerivationPrf.HMACSHA1, 10000, 256 / 8));
  }
  ```
- **Verificação de senha**:
  ```csharp
  private static bool VerificarSenha(string senha, string hashArmazenado, byte[] salt)
  {
      return HashSenha(senha, salt) == hashArmazenado;
  }
  ```
- **Geração de token JWT** (método privado `GerarToken`):
  ```csharp
  private string GerarToken(int userId, string email)
  {
      var jwtSettings = _configuration.GetSection("jwt");
      var secretKey = jwtSettings["secretKey"] ?? throw new InvalidOperationException("Secret key não configurada em appsettings.json");
      var issuer = jwtSettings["issuer"] ?? "ProdutosApi";
      var audience = jwtSettings["audience"] ?? "http://localhost:5000";

      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
      var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      var claims = new[]
      {
          new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
          new Claim(ClaimTypes.Email, email),
          new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
      };

      var token = new JwtSecurityToken(
          issuer: issuer,
          audience: audience,
          claims: claims,
          expires: DateTime.UtcNow.AddHours(8),
          signingCredentials: credentials);

      return new JwtSecurityTokenHandler().WriteToken(token);
  }
  ```
- **RegistrarAsync**: verificar se email já existe → criar salt + hash → salvar → gerar token → retornar.
- **AutenticarAsync**: buscar por email → verificar senha → gerar token → retornar.
- **ObterPorEmailAsync**: busca com `AsNoTracking()`.

### 6. Criar controladores

**`Controllers/AuthController.cs`** (sem `[Authorize]`):
```csharp
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registrar([FromBody] UserCreateDto dto, CancellationToken ct)
    {
        if (dto == null) return BadRequest("Dados inválidos.");

        var existe = await _authService.UsuarioExisteAsync(dto.Email, ct);
        if (existe) return BadRequest("E-mail já cadastrado.");

        var resultado = await _authService.RegistrarAsync(dto, ct);
        return CreatedAtAction(nameof(Autenticar), new { }, new { Token = resultado.Token });
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Autenticar([FromBody] UserLoginDto dto, CancellationToken ct)
    {
        if (dto == null) return BadRequest("Dados inválidos.");

        var resultado = await _authService.AutenticarAsync(dto, ct);
        if (resultado.User == null) return Unauthorized("E-mail ou senha inválidos.");

        return Ok(new { Token = resultado.Token });
    }
}
```

**`Controllers/UsersController.cs`** (com `[Authorize]`):
```csharp
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context) => _context = context;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterTodos(CancellationToken ct)
    {
        var usuarios = await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                IsAdmin = u.IsAdmin,
                Ativo = u.Ativo,
                CriadoEm = u.CriadoEm
            })
            .ToListAsync(ct);

        return Ok(usuarios);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
    {
        var usuario = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                IsAdmin = u.IsAdmin,
                Ativo = u.Ativo,
                CriadoEm = u.CriadoEm
            })
            .FirstOrDefaultAsync(ct);

        if (usuario == null) return NotFound("Usuário não encontrado.");
        return Ok(usuario);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] UserCreateDto dto, CancellationToken ct)
    {
        var usuario = await _context.Users.FindAsync([id], ct);
        if (usuario == null) return NotFound("Usuário não encontrado.");

        usuario.Name = dto.Name.Trim();
        usuario.Email = dto.Email.Trim();
        usuario.IsAdmin = dto.IsAdmin;
        usuario.AtualizadoEm = DateTime.UtcNow;

        // Atualizar senha se informada
        if (!string.IsNullOrEmpty(dto.Senha))
        {
            usuario.Salt = GerarSalt();
            usuario.PasswordHash = HashSenha(dto.Senha, usuario.Salt);
        }

        await _context.SaveChangesAsync(ct);

        return Ok(new UserResponseDto
        {
            Id = usuario.Id,
            Name = usuario.Name,
            Email = usuario.Email,
            IsAdmin = usuario.IsAdmin,
            Ativo = usuario.Ativo,
            CriadoEm = usuario.CriadoEm
        });
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Remover(int id, CancellationToken ct)
    {
        var usuario = await _context.Users.FindAsync([id], ct);
        if (usuario == null) return NotFound("Usuário não encontrado.");

        _context.Users.Remove(usuario);
        await _context.SaveChangesAsync(ct);

        return NoContent();
    }
}
```

> **Nota**: Os métodos `GerarSalt` e `HashSenha` podem ser estáticos privados no controller
> ou extraídos para um serviço auxiliar. Use a mesma lógica do `AuthService`.

### 7. Atualizar AppDbContext

Adicionar ao `AppDbContext`:
```csharp
public DbSet<User> Users => Set<User>();
```

E em `OnModelCreating`:
```csharp
modelBuilder.Entity<User>(entity =>
{
    entity.HasKey(u => u.Id);

    entity.Property(u => u.Name)
        .IsRequired()
        .HasMaxLength(50);

    entity.Property(u => u.Email)
        .IsRequired()
        .HasMaxLength(100);

    entity.HasIndex(u => u.Email)
        .IsUnique();

    entity.Property(u => u.PasswordHash)
        .IsRequired();

    entity.Property(u => u.Salt)
        .IsRequired();
});
```

### 8. Configurar JWT em appsettings.json

Adicionar a seção `jwt`:
```json
{
  "jwt": {
    "secretKey": "MinhaChaveSecretaJWT2024!@#$MinhaChaveSecretaJWT2024!@#$",
    "issuer": "ProdutosApi",
    "audience": "http://localhost:5000"
  }
}
```

### 9. Registrar tudo no Program.cs

Adicionar ao topo:
```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
```

Antes de `builder.Build()`, adicionar configuração JWT:
```csharp
// Configuração JWT Bearer
var jwtSettings = builder.Configuration.GetSection("jwt");
var secretKey = jwtSettings["secretKey"] ?? throw new InvalidOperationException("Secret key JWT não configurada.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["issuer"],
        ValidAudience = jwtSettings["audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// Registrar AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

// Trocar AddAuthorization por AddAuthentication + Authorization
builder.Services.AddAuthorization();
```

Trocar `app.UseAuthorization()` para vir **antes** de `app.MapControllers()` (já está na ordem correta).

Chamar `app.UseAuthentication()` antes de `app.UseAuthorization()`:
```csharp
app.UseAuthentication();
app.UseAuthorization();
```

### 10. Criar testes unitários

**`tests/ProdutosApi.Tests/AuthServiceTests.cs`**:
- Teste: `RegistrarAsync_DeveCriarUsuarioEGerarToken` — cria usuário válido, verifica token retornado não é vazio
- Teste: `RegistrarAsync_DeveFalhar_QuandoEmailDuplicado` — registra duas vezes com mesmo email
- Teste: `AutenticarAsync_DeveRetornarToken_QuandoCredenciaisValidas` — registra e loga
- Teste: `AutenticarAsync_DeveRetornarNull_QuandoSenhaIncorreta` — senha errada retorna null
- Teste: `UsuarioExisteAsync_DeveRetornarTrue_QuandoEmailCadastrado`
- Teste: `GerarToken_DeveRetornarTokenValido` — verifica claims no token

**`tests/ProdutosApi.Tests/UserCreateDtoValidatorTests.cs`**:
- Nome vazio → erro
- Nome > 50 chars → erro
- Email inválido → erro
- Senha < 6 chars → erro
- Dados válidos → sem erro

**`tests/ProdutosApi.Tests/UserLoginDtoValidatorTests.cs`**:
- Email vazio → erro
- Email inválido → erro
- Senha vazia → erro
- Dados válidos → sem erro

### 11. Compilar e testar

```powershell
dotnet build Migracao.sln --nologo -v q
dotnet test tests/ProdutosApi.Tests/ --nologo -v q
```

Corrija erros até build limpo e testes 100% verdes.

## Resumo final

Ao finalizar, relate:

1. **Arquivos criados**: lista completa de novos arquivos
2. **Arquivos modificados**: AppDbContext, appsettings.json, Program.cs, csproj
3. **Endpoints expostos**:
   - `POST /api/Auth/register` — cadastra usuário + retorna token JWT
   - `POST /api/Auth/login` — autentica + retorna token JWT
   - `GET /api/Users` — lista todos (requer auth)
   - `GET /api/Users/{id}` — busca um usuário (requer auth)
   - `PUT /api/Users/{id}` — atualiza usuário (requer auth)
   - `DELETE /api/Users/{id}` — remove usuário (requer auth)
4. **Segurança**: hash PBKDF2 com salt, token JWT com expiração de 8h, bearer auth configurado
5. **Resultado dos testes**: build e testes verdes ou erros encontrados

## Regras importantes

- **NUNCA** use `Microsoft.AspNetCore.Identity` — use hash manual com PBKDF2 (padrão da referência).
- **NUNCA** desvie do padrão de nomenclatura em português do projeto existente.
- **NUNCA** altere os controladores `ProdutosController.cs` ou `CategoriasController.cs` existentes.
- O token JWT deve expirar em **8 horas** (padrão da referência).
- A senha deve ter **mínimo de 6 caracteres**.
- O email deve ser **único** no banco (índice único).
- Use `AsNoTracking()` em todas as leituras.
- Use `Trim()` em strings ao criar/atualizar.
- **Compile e teste de verdade** — só finalize com build e testes verdes.