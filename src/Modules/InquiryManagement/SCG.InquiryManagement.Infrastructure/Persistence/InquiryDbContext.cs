using Microsoft.EntityFrameworkCore;
using SCG.InquiryManagement.Domain.Entities;
using SCG.Infrastructure.Common.Persistence;

namespace SCG.InquiryManagement.Infrastructure.Persistence;

public sealed class InquiryDbContext(DbContextOptions<InquiryDbContext> options)
    : BaseDbContext(options)
{
    public DbSet<Batch> Batches => Set<Batch>();
    public DbSet<BatchTraveler> BatchTravelers => Set<BatchTraveler>();
    public DbSet<Inquiry> Inquiries => Set<Inquiry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("inquiry");
        base.OnModelCreating(modelBuilder);
    }
}
