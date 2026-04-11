using Microsoft.EntityFrameworkCore;
using SCG.AgencyManagement.Infrastructure.Persistence;
using SCG.Application.Abstractions.Services;
using SCG.SharedKernel;

namespace SCG.AgencyManagement.Infrastructure.Services;

internal sealed class WalletServiceAdapter : IWalletService
{
    private readonly AgencyDbContext _db;

    public WalletServiceAdapter(AgencyDbContext db) => _db = db;

    public async Task<Result<decimal>> DebitAsync(
        Guid agencyId, decimal amount, string reference, string? notes, string userId,
        CancellationToken ct = default)
    {
        var wallet = await _db.Wallets
            .FirstOrDefaultAsync(w => w.AgencyId == agencyId, ct);

        if (wallet is null)
            return Result<decimal>.Failure("Agency wallet not found.");

        var debitResult = wallet.Debit(amount, reference, notes, userId);
        if (debitResult.IsFailure)
            return Result<decimal>.Failure(debitResult.Error!);

        await _db.SaveChangesAsync(ct);
        return Result<decimal>.Success(wallet.Balance);
    }
}
