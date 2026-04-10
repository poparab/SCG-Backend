using SCG.AgencyManagement.Application.Abstractions;
using SCG.AgencyManagement.Application.Queries.GetWalletByAgencyId;
using SCG.AgencyManagement.Domain.Enums;
using SCG.Application.Abstractions.Messaging;
using SCG.SharedKernel;

namespace SCG.AgencyManagement.Application.Queries.GetWalletTransactions;

internal sealed class GetWalletTransactionsQueryHandler
    : IQueryHandler<GetWalletTransactionsQuery, PagedResult<WalletTransactionDto>>
{
    private readonly IWalletRepository _walletRepository;

    public GetWalletTransactionsQueryHandler(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<Result<PagedResult<WalletTransactionDto>>> Handle(
        GetWalletTransactionsQuery request, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByAgencyIdAsync(request.AgencyId, cancellationToken);
        if (wallet is null)
            return Result<PagedResult<WalletTransactionDto>>.Failure("Wallet not found for the specified agency.");

        WalletTransactionType? typeFilter = request.TransactionType?.ToLowerInvariant() switch
        {
            "credit" => WalletTransactionType.Credit,
            "debit" => WalletTransactionType.Debit,
            _ => null
        };

        var transactions = await _walletRepository.GetTransactionsAsync(
            wallet.Id, request.From, request.To, typeFilter, request.Page, request.PageSize, cancellationToken);

        var totalCount = await _walletRepository.CountTransactionsAsync(
            wallet.Id, request.From, request.To, typeFilter, cancellationToken);

        var items = transactions.Select(t => new WalletTransactionDto(
            t.Id,
            t.Amount,
            t.Type.ToString(),
            t.ReferenceNumber,
            t.Notes,
            t.CreatedBy,
            t.CreatedAt)).ToList();

        return Result<PagedResult<WalletTransactionDto>>.Success(
            new PagedResult<WalletTransactionDto>(items, totalCount, request.Page, request.PageSize));
    }
}
