---
name: criar-dominio
description:
  Cria um domínio CRUD completo na Produtos API a partir da estrutura de um
  objeto informada pelo usuário (nome da entidade + campos). Gera Model, DTOs,
  Validators (FluentValidation), Service (interface + implementação),
  Controller, registra o DbSet no AppDbContext e cria os testes unitários,
  seguindo exatamente o padrão do domínio Produtos. Ao final compila e roda os
  testes.
---

# Gerador de Domínio CRUD (.NET 8)

Você é um agente especializado em estender a **Produtos API** (solução em
`Migracao.sln`, projeto `src/ProdutosApi`, testes em `tests/ProdutosApi.Tests`).
Sua tarefa é criar um **novo domínio CRUD completo** a partir da estrutura de um
objeto que o usuário descreve, replicando fielmente o padrão já existente no
domínio `Produto`.

## Entrada esperada

O usuário fornece, em linguagem natural ou estruturada:

- O **nome da entidade** no singular (ex.: `Categoria`, `Cliente`, `Pedido`).
- A **lista de campos** com tipo e regras. Exemplo:

  ```
  Entidade: Categoria
  Campos:
    - Nome: string, obrigatório, máx 80
    - Descricao: string, opcional, máx 300
    - Ativo: bool (default true)
  ```

Se o nome da entidade ou os campos não estiverem claros, **pergunte antes de
gerar**. Não invente regras de negócio que não foram pedidas.

## Convenções obrigatórias (siga o padrão do domínio Produto)

Antes de escrever qualquer código, **leia os arquivos de referência** para
copiar o estilo exato (namespaces, nomenclatura em português, organização,
async/CancellationToken, mapeamento manual DTO↔entidade):

- `src/ProdutosApi/Models/Produto.cs`
- `src/ProdutosApi/Dtos/ProdutoCreateDto.cs`, `ProdutoUpdateDto.cs`, `ProdutoResponseDto.cs`
- `src/ProdutosApi/Validators/ProdutoCreateDtoValidator.cs`, `ProdutoUpdateDtoValidator.cs`
- `src/ProdutosApi/Services/IProdutoService.cs`, `ProdutoService.cs`
- `src/ProdutosApi/Controllers/ProdutosController.cs`
- `src/ProdutosApi/Data/AppDbContext.cs`
- `tests/ProdutosApi.Tests/ProdutoServiceTests.cs`, `ProdutoCreateDtoValidatorTests.cs`

Regras de estilo derivadas desse padrão (NÃO desvie sem motivo):

1. **Idioma**: tudo em português (nomes de classes só quando fizer sentido;
   propriedades, mensagens de validação e métodos seguem o que já existe —
   `ObterTodosAsync`, `ObterPorIdAsync`, `CriarAsync`, `AtualizarAsync`,
   `RemoverAsync`).
2. **Entidade** (`Models/`): sempre tem `int Id`, e os campos `bool Ativo`
   (default `true`), `DateTime CriadoEm = DateTime.UtcNow` e
   `DateTime? AtualizadoEm`, além dos campos informados pelo usuário.
3. **DTOs** (`Dtos/`): três arquivos — `XCreateDto` (sem Id/Ativo/datas),
   `XUpdateDto` (inclui `Ativo`), `XResponseDto` (todos os campos de saída,
   incluindo Id e datas).
4. **Validators** (`Validators/`): um para Create e um para Update, com
   `AbstractValidator<T>`, mensagens em português. `NotEmpty` para strings
   obrigatórias, `MaximumLength` conforme limite, regras numéricas conforme
   pedido (ex.: `GreaterThan(0)`, `GreaterThanOrEqualTo(0)`).
5. **Service** (`Services/`): interface `IXService` + `XService`. Usa
   `AppDbContext`, `AsNoTracking()` nas leituras, `Trim()` em strings ao criar/
   atualizar, `CriadoEm`/`AtualizadoEm`, mapeamento manual via método privado
   `MapToResponse`. Métodos assíncronos com `CancellationToken cancellationToken
   = default`. `ObterPorId`/`Atualizar` retornam nulo quando não encontram;
   `Remover` retorna `bool`.
6. **Controller** (`Controllers/`): `[ApiController]`, `[Route("api/[controller]")]`,
   `[Produces("application/json")]`, comentários XML `<summary>` em cada action e
   atributos `[ProducesResponseType]`. Rota `{id:int}`. `POST` usa
   `CreatedAtAction`. Retornos: 200/201/204/400/404 conforme o padrão.
7. **DbContext**: adicione `public DbSet<X> Xs => Set<X>();` e um bloco
   `modelBuilder.Entity<X>(...)` em `OnModelCreating` com `HasKey`, `IsRequired`/
   `HasMaxLength` para strings, `HasPrecision(18, 2)` para `decimal`, e
   `HasIndex` em campos relevantes. **Use `HasPrecision`, nunca `HasColumnType`**
   (o provider é InMemory e não suporta tipos relacionais).
8. **Registro de DI** em `Program.cs`: adicione
   `builder.Services.AddScoped<IXService, XService>();` junto às demais
   registrações. Os validators já são captados por
   `AddValidatorsFromAssemblyContaining` — não precisa registrar um a um.
9. **Testes** (`tests/`): crie `XServiceTests.cs` (banco InMemory isolado por
   teste com `Guid.NewGuid()`, cobrindo criar/obter/listar/atualizar/remover e os
   casos "não encontrado") e `XCreateDtoValidatorTests.cs` (um `[Fact]` válido +
   `[Theory]`/`[Fact]` para cada regra de validação). Use **asserts nativos do
   xUnit** e o `FluentValidation.TestHelper` (`TestValidate`,
   `ShouldHaveValidationErrorFor`, `ShouldNotHaveAnyValidationErrors`).
   **Não use FluentAssertions** (licença comercial paga na v8).

## Procedimento

1. Confirme/parseie a entidade e os campos. Pergunte se algo estiver ambíguo.
2. Leia os arquivos de referência listados acima.
3. Crie os arquivos do domínio em `src/ProdutosApi/` (Model, 3 DTOs, 2
   Validators, interface + Service, Controller).
4. Edite `src/ProdutosApi/Data/AppDbContext.cs` (DbSet + configuração) e
   `src/ProdutosApi/Program.cs` (registro do Service).
5. Crie os dois arquivos de teste em `tests/ProdutosApi.Tests/`.
6. Rode `dotnet build Migracao.sln --nologo -v q` e corrija erros até compilar.
7. Rode `dotnet test Migracao.sln --nologo -v q` e garanta que todos passam.
8. Faça um resumo: arquivos criados/alterados, endpoints expostos
   (`/api/<entidades>`), regras de validação aplicadas e o resultado dos testes.

## Pluralização das rotas

A rota vem de `[controller]` (nome do controller sem o sufixo `Controller`).
Nomeie o controller no plural correto em português (ex.: `CategoriasController`
→ `/api/categorias`, `ClientesController` → `/api/clientes`). Se o plural for
irregular, ajuste o nome do controller para que a rota fique natural.

## Regras de qualidade

- Não quebre o domínio `Produto` existente nem altere arquivos não relacionados.
- Tipos suportados nos campos: `string`, `int`, `long`, `decimal`, `double`,
  `bool`, `DateTime`, `Guid`, enums e versões anuláveis (`?`). Para `decimal`
  configure `HasPrecision`. Para enum, persista como o tipo padrão do EF.
- Compile e teste de verdade — só finalize com build e testes verdes.
- Relate honestamente qualquer ajuste ou limitação encontrada.
