using Microsoft.Extensions.Logging;

namespace SCG.AgencyManagement.Infrastructure.Jobs;

public sealed class WalletLowBalanceAlertJob
{
    private readonly ILogger<WalletLowBalanceAlertJob> _logger;

    public WalletLowBalanceAlertJob(ILogger<WalletLowBalanceAlertJob> logger) => _logger = logger;

    public Task ExecuteAsync()
    {
        _logger.LogInformation("WalletLowBalanceAlertJob: Checking wallets for low balance alerts...");
        // TODO: Query wallets below threshold, queue notification
        return Task.CompletedTask;
    }
}
