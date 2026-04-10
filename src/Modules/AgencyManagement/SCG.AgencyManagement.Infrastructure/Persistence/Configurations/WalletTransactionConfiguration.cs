using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCG.AgencyManagement.Domain.Entities;

namespace SCG.AgencyManagement.Infrastructure.Persistence.Configurations;

internal sealed class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.ToTable("WalletTransactions");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Amount).HasPrecision(18, 2);
        builder.Property(t => t.BalanceAfter).HasPrecision(18, 2);
        builder.Property(t => t.Type).HasConversion<string>().HasMaxLength(10);
        builder.Property(t => t.ReferenceNumber).HasMaxLength(50).IsRequired();
        builder.Property(t => t.Notes).HasMaxLength(500);
        builder.Property(t => t.CreatedBy).HasMaxLength(256).IsRequired();
        builder.Property(t => t.PaymentMethod).HasMaxLength(20);
        builder.Property(t => t.EvidenceFileName).HasMaxLength(256);

        builder.HasIndex(t => t.ReferenceNumber);
        builder.HasIndex(t => t.CreatedAt);
    }
}

