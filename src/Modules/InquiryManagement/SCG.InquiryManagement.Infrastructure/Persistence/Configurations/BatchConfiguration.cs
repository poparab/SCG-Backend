using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCG.InquiryManagement.Domain.Entities;

namespace SCG.InquiryManagement.Infrastructure.Persistence.Configurations;

internal sealed class BatchConfiguration : IEntityTypeConfiguration<Batch>
{
    public void Configure(EntityTypeBuilder<Batch> builder)
    {
        builder.ToTable("Batches");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name).HasMaxLength(200).IsRequired();
        builder.Property(b => b.Notes).HasMaxLength(1000);
        builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(b => b.PaymentReference).HasMaxLength(100);
        builder.Property(b => b.TotalFee).HasPrecision(18, 2);

        builder.HasIndex(b => b.AgencyId);
        builder.HasIndex(b => b.Status);
        builder.HasIndex(b => b.CreatedAt);

        builder.HasMany(b => b.Travelers)
            .WithOne(t => t.Batch)
            .HasForeignKey(t => t.BatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
