using Microsoft.Extensions.Logging;

namespace SCG.InquiryManagement.Infrastructure.Jobs;

public sealed class BatchStatusCheckJob
{
    private readonly ILogger<BatchStatusCheckJob> _logger;

    public BatchStatusCheckJob(ILogger<BatchStatusCheckJob> logger) => _logger = logger;

    public Task ExecuteAsync()
    {
        _logger.LogInformation("BatchStatusCheckJob: Checking batch statuses for pending processing batches...");
        // TODO: Query batches in Processing status, check clearance engine for results
        return Task.CompletedTask;
    }
}
