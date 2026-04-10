using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCG.InquiryManagement.Domain.Entities;

namespace SCG.InquiryManagement.Infrastructure.Persistence.Configurations;

internal sealed class BatchTravelerConfiguration : IEntityTypeConfiguration<BatchTraveler>
{
    public void Configure(EntityTypeBuilder<BatchTraveler> builder)
    {
        builder.ToTable("BatchTravelers");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.FirstNameEn).HasMaxLength(100).IsRequired();
        builder.Property(t => t.LastNameEn).HasMaxLength(100).IsRequired();
        builder.Property(t => t.FirstNameAr).HasMaxLength(100);
        builder.Property(t => t.LastNameAr).HasMaxLength(100);
        builder.Property(t => t.PassportNumber).HasMaxLength(20).IsRequired();
        builder.Property(t => t.NationalityCode).HasMaxLength(3).IsRequired();
        builder.Property(t => t.Gender).HasConversion<string>().HasMaxLength(10);
        builder.Property(t => t.ArrivalAirport).HasMaxLength(100);
        builder.Property(t => t.TransitCountries).HasMaxLength(500);
        builder.Property(t => t.PassportExpiry).IsRequired();
        builder.Property(t => t.DepartureCountry).HasMaxLength(100).IsRequired();
        builder.Property(t => t.PurposeOfTravel).HasMaxLength(50).IsRequired();
        builder.Property(t => t.FlightNumber).HasMaxLength(20);

        builder.HasIndex(t => new { t.BatchId, t.RowIndex });
        builder.HasIndex(t => t.PassportNumber);

        builder.HasOne(t => t.Inquiry)
            .WithMany()
            .HasForeignKey(t => t.InquiryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

