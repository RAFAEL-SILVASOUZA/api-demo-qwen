using Microsoft.EntityFrameworkCore;
using ProdutosApi.Models;

namespace ProdutosApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Produto> Produtos => Set<Produto>();

    public DbSet<Categoria> Categorias => Set<Categoria>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Nome)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.Descricao)
                .HasMaxLength(500);

            entity.Property(p => p.Preco)
                .HasPrecision(18, 2);

            entity.HasIndex(p => p.Nome);

            entity.HasOne(p => p.Categoria)
                .WithMany(c => c.Produtos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Nome)
                .IsRequired()
                .HasMaxLength(80);

            entity.Property(c => c.Descricao)
                .HasMaxLength(300);

            entity.HasIndex(c => c.Nome);
        });

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
    }
}
