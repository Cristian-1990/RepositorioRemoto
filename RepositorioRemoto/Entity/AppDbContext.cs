using Microsoft.EntityFrameworkCore;

namespace RepositorioRemoto.Entity;

/// <summary>
/// Contexto de Entity Framework Core para la base de datos local de usuarios.
/// El proveedor (SQLite o PostgreSQL) se decide al registrarlo, según el perfil.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>Tabla de usuarios.</summary>
    public DbSet<UserEntity> Users => Set<UserEntity>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(e => e.Id);

            // El id lo asigna la API remota: la base de datos NO debe generarlo.
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();

            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(200).IsRequired();

            entity.Property(e => e.Street).HasColumnName("street").HasMaxLength(200);
            entity.Property(e => e.Suite).HasColumnName("suite").HasMaxLength(100);
            entity.Property(e => e.City).HasColumnName("city").HasMaxLength(100);
            entity.Property(e => e.Zipcode).HasColumnName("zipcode").HasMaxLength(20);
            entity.Property(e => e.Lat).HasColumnName("lat").HasMaxLength(50);
            entity.Property(e => e.Lng).HasColumnName("lng").HasMaxLength(50);

            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(50);
            entity.Property(e => e.Website).HasColumnName("website").HasMaxLength(200);

            entity.Property(e => e.CompanyName).HasColumnName("company_name").HasMaxLength(200);
            entity.Property(e => e.CompanyCatchPhrase).HasColumnName("company_catch_phrase").HasMaxLength(300);
            entity.Property(e => e.CompanyBs).HasColumnName("company_bs").HasMaxLength(300);
        });
    }
}