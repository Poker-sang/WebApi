using Microsoft.EntityFrameworkCore;

namespace WebApi.Database;

public class CnnDatabaseContext : DbContext
{
    public CnnDatabaseContext(DbContextOptions<CnnDatabaseContext> options)
        : base(options) { }

    public virtual DbSet<SequentialRecord> SequentialRecord { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<SequentialRecord>(entity =>
        {
            _ = entity.HasKey(e => e.Name);

            _ = entity.ToTable("Sequential");

            _ = entity.Property(e => e.Name)
                .HasMaxLength(32)
                .IsUnicode(false);

            _ = entity.Property(e => e.ParamsName).IsUnicode(false);
            _ = entity.Property(e => e.ParamsType).IsUnicode(false);
            _ = entity.Property(e => e.ParamsRemark).IsUnicode(false);
            _ = entity.Property(e => e.ParamsDefault).IsUnicode(false);

            _ = entity.Property(e => e.ContentJson).IsUnicode(false);

            _ = entity.Property(e => e.CreateTime).HasColumnType("date");

            _ = entity.Property(e => e.Remark).HasMaxLength(256);
        });
}
