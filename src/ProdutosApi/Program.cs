using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using ProdutosApi.Data;
using ProdutosApi.Services;
using ProdutosApi.Validators;

var builder = WebApplication.CreateBuilder(args);

// Banco em memória (substituível por SQL Server/PostgreSQL em produção).
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("ProdutosDb"));

builder.Services.AddScoped<IProdutoService, ProdutoService>();

// Validações com FluentValidation (validação automática a partir dos DTOs).
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ProdutoCreateDtoValidator>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Produtos API",
        Version = "v1",
        Description = "API para cadastro e gerenciamento de produtos."
    });

    // Inclui os comentários XML na documentação do Swagger.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Produtos API v1");
        options.RoutePrefix = string.Empty; // Swagger na raiz: https://localhost:porta/
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Exposto para permitir testes de integração com WebApplicationFactory.
public partial class Program { }
