using Microsoft.EntityFrameworkCore;
using SCG.AgencyManagement.Application.Abstractions;
using SCG.AgencyManagement.Domain.Entities;
using SCG.AgencyManagement.Domain.Enums;

namespace SCG.AgencyManagement.Infrastructure.Persistence;

internal sealed class WalletRepository : IWalletRepository
{
    private readonly AgencyDbContext _db;

    public WalletRepository(AgencyDbContext db) => _db = db;

    public async Task<Wallet?> GetByAgencyIdAsync(Guid agencyId, CancellationToken ct = default)
        => await _db.Wallets
            .FirstOrDefaultAsync(w => w.AgencyId == agencyId, ct);

    public async Task<List<WalletTransaction>> GetTransactionsAsync(
        Guid walletId, DateTime? from, DateTime? to, WalletTransactionType? type,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.WalletTransactions
            .Where(t => t.WalletId == walletId);

        query = ApplyTransactionFilters(query, from, to, type);

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<int> CountTransactionsAsync(
        Guid walletId, DateTime? from, DateTime? to, WalletTransactionType? type,
        CancellationToken ct = default)
    {
        var query = _db.WalletTransactions
            .Where(t => t.WalletId == walletId);

        query = ApplyTransactionFilters(query, from, to, type);

        return await query.CountAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);

    private static IQueryable<WalletTransaction> ApplyTransactionFilters(
        IQueryable<WalletTransaction> query, DateTime? from, DateTime? to, WalletTransactionType? type)
    {
        if (from.HasValue)
            query = query.Where(t => t.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(t => t.CreatedAt <= to.Value);

        if (type.HasValue)
            query = query.Where(t => t.Type == type.Value);

        return query;
    }
}
