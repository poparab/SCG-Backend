using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCG.InquiryManagement.Domain.Entities;

namespace SCG.InquiryManagement.Infrastructure.Persistence.Configurations;

internal sealed class InquiryConfiguration : IEntityTypeConfiguration<Inquiry>
{
    public void Configure(EntityTypeBuilder<Inquiry> builder)
    {
        builder.ToTable("Inquiries");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ReferenceNumber).HasMaxLength(30).IsRequired();
        builder.Property(i => i.FirstNameEn).HasMaxLength(100).IsRequired();
        builder.Property(i => i.LastNameEn).HasMaxLength(100).IsRequired();
        builder.Property(i => i.FirstNameAr).HasMaxLength(100);
        builder.Property(i => i.LastNameAr).HasMaxLength(100);
        builder.Property(i => i.PassportNumber).HasMaxLength(20).IsRequired();
        builder.Property(i => i.NationalityCode).HasMaxLength(3).IsRequired();
        builder.Property(i => i.Gender).HasConversion<string>().HasMaxLength(10);
        builder.Property(i => i.ArrivalAirport).HasMaxLength(100);
        builder.Property(i => i.TransitCountries).HasMaxLength(500);
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.ResultCode).HasMaxLength(50);
        builder.Property(i => i.RejectionReason).HasMaxLength(1000);
        builder.Property(i => i.DocumentUrl).HasMaxLength(500);
        builder.Property(i => i.Fee).HasPrecision(18, 2);
        builder.Property(i => i.PaymentReference).HasMaxLength(100);

        builder.HasIndex(i => i.ReferenceNumber).IsUnique();
        builder.HasIndex(i => i.AgencyId);
        builder.HasIndex(i => i.BatchId);
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.PassportNumber);
        builder.HasIndex(i => i.NationalityCode);
    }
}
