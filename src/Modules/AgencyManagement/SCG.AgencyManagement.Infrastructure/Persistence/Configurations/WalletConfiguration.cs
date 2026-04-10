using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCG.AgencyManagement.Domain.Entities;

namespace SCG.AgencyManagement.Infrastructure.Persistence.Configurations;

internal sealed class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("Wallets");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Balance).HasPrecision(18, 2);
        builder.Property(w => w.LowBalanceThreshold).HasPrecision(18, 2);
        builder.Property(w => w.Currency).HasMaxLength(3).HasDefaultValue("USD");

        builder.HasIndex(w => w.AgencyId).IsUnique();

        builder.HasMany(w => w.Transactions)
            .WithOne(t => t.Wallet)
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
