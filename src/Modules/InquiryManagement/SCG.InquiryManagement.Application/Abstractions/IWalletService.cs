using SCG.SharedKernel;

namespace SCG.InquiryManagement.Application.Abstractions;

public interface IWalletService
{
    Task<Result<decimal>> DebitAsync(Guid agencyId, decimal amount, string reference, string? notes, string userId, CancellationToken ct = default);
}
