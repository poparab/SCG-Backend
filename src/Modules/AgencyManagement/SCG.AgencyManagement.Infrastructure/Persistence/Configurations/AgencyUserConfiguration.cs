using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCG.AgencyManagement.Domain.Entities;

namespace SCG.AgencyManagement.Infrastructure.Persistence.Configurations;

internal sealed class AgencyUserConfiguration : IEntityTypeConfiguration<AgencyUser>
{
    public void Configure(EntityTypeBuilder<AgencyUser> builder)
    {
        builder.ToTable("AgencyUsers");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstNameAr).HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastNameAr).HasMaxLength(100).IsRequired();
        builder.Property(u => u.FirstNameEn).HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastNameEn).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.Phone).HasMaxLength(20).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(u => u.CountryCode).HasMaxLength(5);
        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => new { u.AgencyId, u.Role });
    }
}
