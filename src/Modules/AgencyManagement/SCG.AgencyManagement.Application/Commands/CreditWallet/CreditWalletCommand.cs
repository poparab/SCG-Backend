using SCG.Application.Abstractions.Messaging;

namespace SCG.AgencyManagement.Application.Commands.CreditWallet;

public sealed record CreditWalletCommand(
    Guid AgencyId,
    decimal Amount,
    string PaymentMethod,
    string ReferenceNumber,
    string? Notes,
    string? EvidenceFileName) : ICommand<CreditWalletResponse>;

public sealed record CreditWalletResponse(decimal NewBalance, Guid TransactionId);
