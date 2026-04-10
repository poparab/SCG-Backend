using Microsoft.EntityFrameworkCore;
using SCG.AgencyManagement.Domain.Entities;
using SCG.Infrastructure.Common.Persistence;

namespace SCG.AgencyManagement.Infrastructure.Persistence;

public sealed class AgencyDbContext(DbContextOptions<AgencyDbContext> options)
    : BaseDbContext(options)
{
    public DbSet<Agency> Agencies => Set<Agency>();
    public DbSet<AgencyUser> AgencyUsers => Set<AgencyUser>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("agency");
        base.OnModelCreating(modelBuilder);
    }
}
