using System.Reflection;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using ProdutosApi.Data;
using ProdutosApi.Services;
using ProdutosApi.Validators;

var builder = WebApplication.CreateBuilder(args);

// Banco em memória (substituível por SQL Server/PostgreSQL em produção).
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("ProdutosDb"));

builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Validações com FluentValidation (validação automática a partir dos DTOs).
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ProdutoCreateDtoValidator>();

builder.Services.AddControllers();

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

builder.Services.AddAuthorization();

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Exposto para permitir testes de integração com WebApplicationFactory.
public partial class Program { }
