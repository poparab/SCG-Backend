using Microsoft.EntityFrameworkCore;
using SCG.Identity.Domain.Entities;
using SCG.Infrastructure.Common.Persistence;

namespace SCG.Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : BaseDbContext(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identity");
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AdminUser>(b =>
        {
            b.ToTable("AdminUsers");
            b.HasKey(a => a.Id);
            b.Property(a => a.Id).ValueGeneratedNever();
            b.Property(a => a.FullName).HasMaxLength(200).IsRequired();
            b.Property(a => a.Email).HasMaxLength(256).IsRequired();
            b.HasIndex(a => a.Email).IsUnique();
            b.Property(a => a.PasswordHash).HasMaxLength(500).IsRequired();
            b.Property(a => a.Role).HasMaxLength(50).IsRequired();

            // Seed default admin — password: Admin@1234
            b.HasData(new
            {
                Id = Guid.Parse("a0000000-0000-0000-0000-000000000001"),
                FullName = "System Administrator",
                Email = "admin@scg.gov.eg",
                PasswordHash = "$2a$11$Dh0ieNb/t0vIcYouclXyEOFfK/M5HhnxY8/A2GEc6y0BOAU0PkXBS",
                Role = "SuperAdmin",
                IsActive = true,
                LastLoginAt = (DateTime?)null,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = (DateTime?)null
            });
        });
    }
}
