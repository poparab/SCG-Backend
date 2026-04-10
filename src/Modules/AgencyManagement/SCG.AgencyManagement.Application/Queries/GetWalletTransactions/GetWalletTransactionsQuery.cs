using SCG.AgencyManagement.Application.Queries.GetWalletByAgencyId;
using SCG.AgencyManagement.Domain.Enums;
using SCG.Application.Abstractions.Messaging;
using SCG.SharedKernel;

namespace SCG.AgencyManagement.Application.Queries.GetWalletTransactions;

public sealed record GetWalletTransactionsQuery(
    Guid AgencyId,
    DateTime? From,
    DateTime? To,
    string? TransactionType,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<WalletTransactionDto>>;
