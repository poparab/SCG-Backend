using SCG.AgencyManagement.Domain.Enums;
using SCG.SharedKernel;

namespace SCG.AgencyManagement.Domain.Entities;

public sealed class WalletTransaction : Entity<Guid>
{
    public Guid WalletId { get; private set; }
    public decimal Amount { get; private set; }
    public WalletTransactionType Type { get; private set; }
    public string ReferenceNumber { get; private set; } = default!;
    public string? Notes { get; private set; }
    public string CreatedBy { get; private set; } = default!;
    public decimal BalanceAfter { get; private set; }
    public string? PaymentMethod { get; private set; }
    public string? EvidenceFileName { get; private set; }

    // Navigation
    public Wallet Wallet { get; private set; } = default!;

    private WalletTransaction() { } // EF

    internal static WalletTransaction CreateCredit(
        Guid walletId, decimal amount, string referenceNumber, string? notes, string createdBy,
        string? paymentMethod = null, string? evidenceFileName = null)
    {
        return new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = walletId,
            Amount = amount,
            Type = WalletTransactionType.Credit,
            ReferenceNumber = referenceNumber,
            Notes = notes,
            CreatedBy = createdBy,
            PaymentMethod = paymentMethod,
            EvidenceFileName = evidenceFileName
        };
    }

    internal static WalletTransaction CreateDebit(
        Guid walletId, decimal amount, string referenceNumber, string? notes, string createdBy)
    {
        return new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = walletId,
            Amount = amount,
            Type = WalletTransactionType.Debit,
            ReferenceNumber = referenceNumber,
            Notes = notes,
            CreatedBy = createdBy
        };
    }
}
