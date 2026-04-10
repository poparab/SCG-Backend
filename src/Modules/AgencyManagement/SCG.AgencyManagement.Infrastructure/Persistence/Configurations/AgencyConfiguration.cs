using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCG.AgencyManagement.Domain.Entities;

namespace SCG.AgencyManagement.Infrastructure.Persistence.Configurations;

internal sealed class AgencyConfiguration : IEntityTypeConfiguration<Agency>
{
    public void Configure(EntityTypeBuilder<Agency> builder)
    {
        builder.ToTable("Agencies");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(a => a.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Email).HasMaxLength(256).IsRequired();
        builder.Property(a => a.Phone).HasMaxLength(20).IsRequired();
        builder.Property(a => a.CommercialLicenseNumber).HasMaxLength(50).IsRequired();
        builder.Property(a => a.CommercialLicenseUrl).HasMaxLength(500);
        builder.Property(a => a.Address).HasMaxLength(500);
        builder.Property(a => a.RejectionReason).HasMaxLength(1000);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(a => a.Email).IsUnique();
        builder.HasIndex(a => a.CommercialLicenseNumber).IsUnique();
        builder.HasIndex(a => a.Status);

        builder.HasOne(a => a.Wallet)
            .WithOne(w => w.Agency)
            .HasForeignKey<Wallet>(w => w.AgencyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Users)
            .WithOne(u => u.Agency)
            .HasForeignKey(u => u.AgencyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
