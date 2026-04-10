using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCG.Rules.Domain.Entities;

namespace SCG.Rules.Infrastructure.Persistence.Configurations;

internal sealed class InquiryTypeConfiguration : IEntityTypeConfiguration<InquiryType>
{
    public void Configure(EntityTypeBuilder<InquiryType> builder)
    {
        builder.ToTable("InquiryTypes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DescriptionAr).HasMaxLength(500);
        builder.Property(x => x.DescriptionEn).HasMaxLength(500);
    }
}

internal sealed class NationalityConfiguration : IEntityTypeConfiguration<Nationality>
{
    public void Configure(EntityTypeBuilder<Nationality> builder)
    {
        builder.ToTable("Nationalities");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(3).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();

        builder.HasMany(x => x.InquiryTypes)
            .WithOne(x => x.Nationality)
            .HasForeignKey(x => x.NationalityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class NationalityInquiryTypeConfiguration : IEntityTypeConfiguration<NationalityInquiryType>
{
    public void Configure(EntityTypeBuilder<NationalityInquiryType> builder)
    {
        builder.ToTable("NationalityInquiryTypes");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.NationalityId, x.InquiryTypeId }).IsUnique();

        builder.HasOne(x => x.InquiryType)
            .WithMany()
            .HasForeignKey(x => x.InquiryTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PricingConfiguration : IEntityTypeConfiguration<Pricing>
{
    public void Configure(EntityTypeBuilder<Pricing> builder)
    {
        builder.ToTable("Pricings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Fee).HasPrecision(18, 2);
        builder.Property(x => x.Currency).HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(x => x.NationalityCode).HasMaxLength(3);

        builder.HasIndex(x => new { x.InquiryTypeId, x.IsActive });

        builder.HasOne(x => x.InquiryType)
            .WithMany()
            .HasForeignKey(x => x.InquiryTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class AgencyCategoryConfiguration : IEntityTypeConfiguration<AgencyCategory>
{
    public void Configure(EntityTypeBuilder<AgencyCategory> builder)
    {
        builder.ToTable("AgencyCategories");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DescriptionAr).HasMaxLength(500);
        builder.Property(x => x.DescriptionEn).HasMaxLength(500);
    }
}

internal sealed class SubmissionWindowConfiguration : IEntityTypeConfiguration<SubmissionWindow>
{
    public void Configure(EntityTypeBuilder<SubmissionWindow> builder)
    {
        builder.ToTable("SubmissionWindows");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DayOfWeek).HasConversion<string>().HasMaxLength(10);

        builder.HasIndex(x => new { x.DayOfWeek, x.IsActive });
    }
}

internal sealed class SystemAnnouncementConfiguration : IEntityTypeConfiguration<SystemAnnouncement>
{
    public void Configure(EntityTypeBuilder<SystemAnnouncement> builder)
    {
        builder.ToTable("SystemAnnouncements");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TitleAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TitleEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.MessageAr).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.MessageEn).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Severity).HasMaxLength(10).HasDefaultValue("info");
    }
}

internal sealed class AgencyNationalityConfiguration : IEntityTypeConfiguration<AgencyNationality>
{
    public void Configure(EntityTypeBuilder<AgencyNationality> builder)
    {
        builder.ToTable("AgencyNationalities");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomFee).HasPrecision(18, 2);

        builder.HasIndex(x => new { x.AgencyId, x.NationalityId }).IsUnique();

        builder.HasOne(x => x.Nationality)
            .WithMany()
            .HasForeignKey(x => x.NationalityId)
            .OnDelete(DeleteBehavior.Cascade);

        // No FK to Agency — cross-module reference
        builder.HasIndex(x => x.AgencyId);
    }
}
