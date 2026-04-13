using Microsoft.EntityFrameworkCore;

namespace SCG.Infrastructure.Common.Persistence;

public sealed class AuditDbContext(DbContextOptions<AuditDbContext> options) : DbContext(options)
{
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("audit");

        modelBuilder.Entity<AuditLog>(b =>
        {
            b.ToTable("AuditLogs");
            b.HasKey(a => a.Id);
            b.Property(a => a.Id).ValueGeneratedNever();
            b.Property(a => a.UserId).HasMaxLength(256);
            b.Property(a => a.UserEmail).HasMaxLength(256);
            b.Property(a => a.Action).HasMaxLength(50).IsRequired();
            b.Property(a => a.EntityType).HasMaxLength(200).IsRequired();
            b.Property(a => a.EntityId).HasMaxLength(200).IsRequired();
            b.Property(a => a.IpAddress).HasMaxLength(45);
            b.HasIndex(a => a.UserEmail);
            b.HasIndex(a => a.EntityType);
            b.HasIndex(a => a.Timestamp);
        });
    }
}
