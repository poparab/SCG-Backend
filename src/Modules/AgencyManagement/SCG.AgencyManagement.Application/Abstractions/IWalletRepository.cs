using SCG.AgencyManagement.Domain.Entities;
using SCG.AgencyManagement.Domain.Enums;

namespace SCG.AgencyManagement.Application.Abstractions;

public interface IWalletRepository
{
    Task<Wallet?> GetByAgencyIdAsync(Guid agencyId, CancellationToken ct = default);
    Task<List<WalletTransaction>> GetTransactionsAsync(Guid walletId, DateTime? from, DateTime? to, WalletTransactionType? type, int page, int pageSize, CancellationToken ct = default);
    Task<int> CountTransactionsAsync(Guid walletId, DateTime? from, DateTime? to, WalletTransactionType? type, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
