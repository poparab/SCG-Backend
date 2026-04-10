using Microsoft.EntityFrameworkCore;
using SCG.Infrastructure.Common.Persistence;
using SCG.Rules.Domain.Entities;

namespace SCG.Rules.Infrastructure.Persistence;

public sealed class RulesDbContext(DbContextOptions<RulesDbContext> options)
    : BaseDbContext(options)
{
    public DbSet<InquiryType> InquiryTypes => Set<InquiryType>();
    public DbSet<Nationality> Nationalities => Set<Nationality>();
    public DbSet<NationalityInquiryType> NationalityInquiryTypes => Set<NationalityInquiryType>();
    public DbSet<Pricing> Pricings => Set<Pricing>();
    public DbSet<AgencyCategory> AgencyCategories => Set<AgencyCategory>();
    public DbSet<AgencyNationality> AgencyNationalities => Set<AgencyNationality>();
    public DbSet<SubmissionWindow> SubmissionWindows => Set<SubmissionWindow>();
    public DbSet<SystemAnnouncement> SystemAnnouncements => Set<SystemAnnouncement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("rules");
        base.OnModelCreating(modelBuilder);

        // Seed default Security Clearance inquiry type
        modelBuilder.Entity<InquiryType>().HasData(new
        {
            Id = WellKnownIds.SecurityClearanceInquiryTypeId,
            NameAr = "تصريح أمني",
            NameEn = "Security Clearance",
            DescriptionAr = "طلب تصريح أمني للمسافرين",
            DescriptionEn = "Security clearance inquiry for travelers",
            IsActive = true,
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
