using SCG.SharedKernel;

namespace SCG.Application.Abstractions.Services;

public interface IWalletService
{
    Task<Result<decimal>> DebitAsync(Guid agencyId, decimal amount, string reference, string? notes, string userId, CancellationToken ct = default);
}
