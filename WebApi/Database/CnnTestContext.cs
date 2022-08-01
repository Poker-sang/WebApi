using Microsoft.EntityFrameworkCore;

namespace WebApi.Database;

public partial class CnnTestContext : DbContext
{
    private readonly string _connectionString;
    public CnnTestContext(string connectionString) => _connectionString = connectionString;

    public CnnTestContext(string connectionString, DbContextOptions<CnnTestContext> options)
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

            _ = entity.ToTable("Sequential");

            _ = entity.Property(e => e.Name)
                .HasMaxLength(32)
                .IsUnicode(false);

            _ = entity.Property(e => e.ContentJson).IsUnicode(false);

            _ = entity.Property(e => e.CreateTime).HasColumnType("date");

            _ = entity.Property(e => e.Remark).HasMaxLength(256);
        });
}
