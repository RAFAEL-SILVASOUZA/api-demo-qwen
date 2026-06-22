# Produtos API

API REST em .NET 8 para cadastro e gerenciamento de produtos, com validações (FluentValidation), documentação Swagger e testes unitários (xUnit).

## Estrutura

```
Migracao.sln
├── src/ProdutosApi/            # API Web (.NET 8)
│   ├── Controllers/            # ProdutosController (endpoints REST)
│   ├── Data/                   # AppDbContext (EF Core InMemory)
│   ├── Dtos/                   # DTOs de entrada/saída
│   ├── Models/                 # Entidade Produto
│   ├── Services/               # Regra de negócio (IProdutoService)
│   ├── Validators/             # Regras de validação FluentValidation
│   └── Program.cs              # Configuração (DI, Swagger, validação)
└── tests/ProdutosApi.Tests/    # Testes unitários (xUnit)
    ├── ProdutoServiceTests.cs
    └── ProdutoCreateDtoValidatorTests.cs
```

## Como executar

```bash
dotnet run --project src/ProdutosApi
```

A interface do Swagger abre na raiz (ex.: `http://localhost:5199/`).

## Como testar

```bash
dotnet test
```

## Endpoints

| Método | Rota                  | Descrição                  |
|--------|-----------------------|----------------------------|
| GET    | `/api/produtos`       | Lista todos os produtos    |
| GET    | `/api/produtos/{id}`  | Obtém um produto por id    |
| POST   | `/api/produtos`       | Cadastra um produto        |
| PUT    | `/api/produtos/{id}`  | Atualiza um produto        |
| DELETE | `/api/produtos/{id}`  | Remove um produto          |

### Exemplo de payload (POST)

```json
{
  "nome": "Notebook",
  "descricao": "16GB RAM",
  "preco": 4500.00,
  "quantidadeEmEstoque": 7
}
```

## Validações

- **Nome**: obrigatório, máximo 100 caracteres
- **Descrição**: opcional, máximo 500 caracteres
- **Preço**: maior que zero
- **Quantidade em estoque**: não pode ser negativa

Erros de validação retornam `400 Bad Request` no formato `ProblemDetails`.

## Observações

- A persistência usa **EF Core InMemory** (dados não sobrevivem ao reinício).
  Para produção, troque o provider em `Program.cs` por SQL Server/PostgreSQL.
