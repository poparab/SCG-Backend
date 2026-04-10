using SCG.AgencyManagement.Application.Abstractions;
using SCG.Application.Abstractions.Messaging;
using SCG.SharedKernel;

namespace SCG.AgencyManagement.Application.Queries.GetWalletByAgencyId;

internal sealed class GetWalletQueryHandler : IQueryHandler<GetWalletQuery, WalletDto>
{
    private readonly IWalletRepository _walletRepository;

    public GetWalletQueryHandler(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<Result<WalletDto>> Handle(GetWalletQuery request, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByAgencyIdAsync(request.AgencyId, cancellationToken);
        if (wallet is null)
            return Result<WalletDto>.Failure("Wallet not found for the specified agency.");

        var recentTransactions = await _walletRepository.GetTransactionsAsync(
            wallet.Id, null, null, null, 1, 10, cancellationToken);

        var transactionDtos = recentTransactions.Select(t => new WalletTransactionDto(
            t.Id,
            t.Amount,
            t.Type.ToString(),
            t.ReferenceNumber,
            t.Notes,
            t.CreatedBy,
            t.CreatedAt)).ToList();

        return Result<WalletDto>.Success(new WalletDto(
            wallet.Id,
            wallet.Balance,
            wallet.Currency,
            wallet.LowBalanceThreshold,
            wallet.IsLowBalance,
            transactionDtos));
    }
}
