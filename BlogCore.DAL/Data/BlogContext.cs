namespace BlogCore.DAL.Data;

using BlogCore.DAL.Models;
using Microsoft.EntityFrameworkCore;

public class BlogContext : DbContext
{
    // Konstruktor pozwalaj¹cy na wstrzykniêcie konfiguracji (np. z Testcontainers) 
    public BlogContext(DbContextOptions<BlogContext> options) : base(options)
    {
    }

    // Definicje tabel w bazie danych 
    public DbSet<Post> Posts { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Konfiguracja modelu Post 
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(p => p.Id); // Definicja klucza g³ównego 

            // SQL Server sam generuje ID (zalecane przy Bogus):
            entity.Property(p => p.Id).ValueGeneratedOnAdd();
            entity.Property(p => p.Author).IsRequired();

            entity.HasMany(e => e.Comments)
            .WithOne(e=>e.Post)
            .HasForeignKey(e => e.PostId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        });

        // Konfiguracja modelu Comment 
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(p => p.Id); // Definicja klucza g³ównego 

            // SQL Server sam generuje ID (zalecane przy Bogus):
            entity.Property(p => p.Id).ValueGeneratedOnAdd();

            entity.Property(p => p.PostId).IsRequired();
            entity.Property(p => p.Content).IsRequired();


        });
    }
}

