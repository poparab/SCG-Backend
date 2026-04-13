using SCG.AgencyManagement.Application.Abstractions;
using SCG.Application.Abstractions.Messaging;
using SCG.SharedKernel;

namespace SCG.AgencyManagement.Application.Commands.CreditWallet;

public sealed class CreditWalletCommandHandler : ICommandHandler<CreditWalletCommand, CreditWalletResponse>
{
    private readonly IWalletRepository _walletRepository;
    private const string AutoReferencePrefix = "ADMIN-TOPUP-";

    public CreditWalletCommandHandler(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<Result<CreditWalletResponse>> Handle(CreditWalletCommand request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
            return Result<CreditWalletResponse>.Failure("Credit amount must be greater than zero.");

        string[] validMethods = ["Cash", "BankTransfer", "Cheque"];
        if (!validMethods.Contains(request.PaymentMethod, StringComparer.OrdinalIgnoreCase))
            return Result<CreditWalletResponse>.Failure("Payment method must be one of: Cash, BankTransfer, Cheque.");

        var wallet = await _walletRepository.GetByAgencyIdAsync(request.AgencyId, cancellationToken);
        if (wallet is null)
            return Result<CreditWalletResponse>.Failure("Wallet not found for the specified agency.");

        var referenceNumber = NormalizeReferenceNumber(request.ReferenceNumber);

        var transaction = wallet.Credit(
            request.Amount, referenceNumber, request.Notes, "Admin",
            request.PaymentMethod, request.EvidenceFileName);
        await _walletRepository.SaveChangesAsync(cancellationToken);

        return Result<CreditWalletResponse>.Success(
            new CreditWalletResponse(wallet.Balance, transaction.Id));
    }

    private static string NormalizeReferenceNumber(string referenceNumber)
    {
        if (!string.IsNullOrWhiteSpace(referenceNumber))
        {
            return referenceNumber.Trim();
        }

        return $"{AutoReferencePrefix}{Guid.NewGuid():N}";
    }
}
