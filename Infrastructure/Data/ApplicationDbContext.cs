using Microsoft.EntityFrameworkCore;
using InventorySaaSBackend.Models;
using InventorySaaSBackend.Domain.Entity;

namespace InventorySaaSBackend.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Empresas> Empresas { get; set; }
    public DbSet<Productos> Productos { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Almacenes> Almacenes { get; set; }
    public DbSet<Stock> Stock { get; set; }
    public DbSet<Movimientos> Movimientos { get; set; }
    public DbSet<UnidadMedida> Unidades { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("inventario");

        // ===== EMPRESAS (Shared) =====
        modelBuilder.Entity<Empresas>(entity =>
        {
            entity.ToTable("Empresas", "shared");
            entity.HasKey(e => e.IdEmpresa);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.HasIndex(e => e.Nombre);
        });

        // ===== CATEGORIAS =====
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.ToTable("Categorias", "inventario");
            entity.HasKey(e => e.IdCategoria);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.HasOne<Empresas>()
                .WithMany()
                .HasForeignKey(e => e.IdEmpresa)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.IdEmpresa, e.Nombre }).IsUnique();
        });

        // ===== UNIDADES =====
        modelBuilder.Entity<UnidadMedida>(entity =>
        {
            entity.ToTable("Unidades", "inventario");
            entity.HasKey(e => e.IdUnidad);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Abreviatura).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Activo).HasDefaultValue(true);

            entity.HasOne<Empresas>()
                .WithMany()
                .HasForeignKey(e => e.IdEmpresa)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.IdEmpresa, e.Nombre }).IsUnique();
        });

        // ===== PRODUCTOS =====
        modelBuilder.Entity<Productos>(entity =>
        {
            entity.ToTable("Productos", "inventario");
            entity.HasKey(e => e.IdProducto);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Sku).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.PrecioVenta).HasColumnType("numeric(18,2)").HasDefaultValue(0m);
            entity.Property(e => e.Agotado86).HasDefaultValue(false);

            entity.HasOne<Empresas>()
                .WithMany()
                .HasForeignKey(e => e.IdEmpresa)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Categoria>()
                .WithMany()
                .HasForeignKey(e => e.IdCategoria)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<UnidadMedida>()
                .WithMany()
                .HasForeignKey(e => e.IdUnidad)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.IdEmpresa, e.Sku }).IsUnique();

            entity.HasIndex(e => e.IdCategoria);
            entity.HasIndex(e => e.IdUnidad);
            entity.HasIndex(e => e.Activo);
        });

        // ===== ALMACENES =====
        modelBuilder.Entity<Almacenes>(entity =>
        {
            entity.ToTable("Almacenes", "inventario");
            entity.HasKey(e => e.IdAlmacen);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.HasOne<Empresas>()
                .WithMany()
                .HasForeignKey(e => e.IdEmpresa)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.IdEmpresa);
        });

        // ===== STOCK =====
        modelBuilder.Entity<Stock>(entity =>
        {
            entity.ToTable("Stock", "inventario");
            entity.HasKey(e => e.IdStock);
            entity.Property(e => e.Cantidad).HasDefaultValue(0);

            entity.HasOne<Empresas>()
                .WithMany()
                .HasForeignKey(e => e.IdEmpresa)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Productos>()
                .WithMany()
                .HasForeignKey(e => e.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Almacenes>()
                .WithMany()
                .HasForeignKey(e => e.IdAlmacen)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.IdProducto, e.IdAlmacen }).IsUnique();

            entity.HasIndex(e => e.IdEmpresa);
            entity.HasIndex(e => e.IdProducto);
            entity.HasIndex(e => e.IdAlmacen);

            entity.ToTable(t => t.HasCheckConstraint("CK_Stock_Cantidad", "cantidad >= 0"));
        });

        // ===== MOVIMIENTOS (Kardex / Audit Trail) =====
        modelBuilder.Entity<Movimientos>(entity =>
        {
            entity.ToTable("Movimientos", "inventario");
            entity.HasKey(e => e.IdMovimiento);
            entity.Property(e => e.Tipo).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Fecha).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne<Empresas>()
                .WithMany()
                .HasForeignKey(e => e.IdEmpresa)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Productos>()
                .WithMany()
                .HasForeignKey(e => e.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Almacenes>()
                .WithMany()
                .HasForeignKey(e => e.IdAlmacen)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.IdEmpresa);
            entity.HasIndex(e => e.IdProducto);
            entity.HasIndex(e => e.IdAlmacen);
            entity.HasIndex(e => e.Fecha);
            entity.HasIndex(e => e.Tipo);
        });

    }
}