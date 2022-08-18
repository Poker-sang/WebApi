using Microsoft.EntityFrameworkCore;

namespace WebApi.Database;

public class CnnDatabaseContext : DbContext
{
    private readonly string _connectionString;
    public CnnDatabaseContext(string connectionString) => _connectionString = connectionString;

    public CnnDatabaseContext(string connectionString, DbContextOptions<CnnDatabaseContext> options)
        : base(options)
        => _connectionString = connectionString;

    public virtual DbSet<Sequential> SequentialRecord { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            _ = optionsBuilder.UseSqlServer(_connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Sequential>(entity =>
        {
            _ = entity.HasKey(e => e.Name);

            _ = entity.ToTable("CnnTable");

            _ = entity.Property(e => e.Name)
                .HasMaxLength(32)
                .IsUnicode(false);

            _ = entity.Property(e => e.ParamsJson).IsUnicode(false);
            
            _ = entity.Property(e => e.ContentJson).IsUnicode(false);

            _ = entity.Property(e => e.CreateTime).HasColumnType("date");

            _ = entity.Property(e => e.Remark).HasMaxLength(256);
        });
}
