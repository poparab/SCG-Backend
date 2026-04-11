using Microsoft.Extensions.Logging;

namespace SCG.Notification.Infrastructure.Jobs;

public sealed class NotificationDispatchJob
{
    private readonly ILogger<NotificationDispatchJob> _logger;

    public NotificationDispatchJob(ILogger<NotificationDispatchJob> logger) => _logger = logger;

    public Task ExecuteAsync()
    {
        _logger.LogInformation("NotificationDispatchJob: Processing pending notification queue...");
        // TODO: Query pending notifications and dispatch via email/SMS
        return Task.CompletedTask;
    }
}
