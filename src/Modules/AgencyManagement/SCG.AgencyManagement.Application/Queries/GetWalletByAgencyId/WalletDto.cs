namespace SCG.AgencyManagement.Application.Queries.GetWalletByAgencyId;

public sealed record WalletDto(
    Guid WalletId,
    decimal Balance,
    string Currency,
    decimal LowBalanceThreshold,
    bool IsLowBalance,
    IReadOnlyList<WalletTransactionDto> RecentTransactions);

public sealed record WalletTransactionDto(
    Guid Id,
    decimal Amount,
    string Type,
    string ReferenceNumber,
    string? Notes,
    string CreatedBy,
    DateTime CreatedAt);
