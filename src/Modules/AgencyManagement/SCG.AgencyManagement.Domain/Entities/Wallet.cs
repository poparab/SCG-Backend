using SCG.AgencyManagement.Domain.Events;
using SCG.SharedKernel;

namespace SCG.AgencyManagement.Domain.Entities;

public sealed class Wallet : Entity<Guid>
{
    public Guid AgencyId { get; private set; }
    public decimal Balance { get; private set; }
    public decimal LowBalanceThreshold { get; private set; } = 100m;
    public string Currency { get; private set; } = "USD";

    // Navigation
    public Agency Agency { get; private set; } = default!;
    private readonly List<WalletTransaction> _transactions = [];
    public IReadOnlyList<WalletTransaction> Transactions => _transactions.AsReadOnly();

    private Wallet() { } // EF

    public static Wallet Create(Guid agencyId, decimal lowBalanceThreshold = 100m)
    {
        return new Wallet
        {
            Id = Guid.NewGuid(),
            AgencyId = agencyId,
            Balance = 0m,
            LowBalanceThreshold = lowBalanceThreshold
        };
    }

    public WalletTransaction Credit(decimal amount, string referenceNumber, string? notes, string createdBy,
        string? paymentMethod = null, string? evidenceFileName = null)
    {
        Balance += amount;
        var txn = WalletTransaction.CreateCredit(Id, amount, referenceNumber, notes, createdBy, paymentMethod, evidenceFileName);
        _transactions.Add(txn);
        return txn;
    }

    public Result<WalletTransaction> Debit(decimal amount, string referenceNumber, string? notes, string createdBy)
    {
        if (Balance < amount)
            return Result<WalletTransaction>.Failure("Insufficient wallet balance.");

        Balance -= amount;
        var txn = WalletTransaction.CreateDebit(Id, amount, referenceNumber, notes, createdBy);
        _transactions.Add(txn);
        RaiseDomainEvent(new WalletDebitedDomainEvent(Id, AgencyId, amount, referenceNumber));
        return Result<WalletTransaction>.Success(txn);
    }

    public bool IsLowBalance => Balance <= LowBalanceThreshold;

    public void UpdateThreshold(decimal threshold) => LowBalanceThreshold = threshold;
}
