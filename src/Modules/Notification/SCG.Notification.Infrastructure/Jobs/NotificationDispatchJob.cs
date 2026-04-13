using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SCG.Notification.Application.Services;
using SCG.Notification.Infrastructure.Persistence;

namespace SCG.Notification.Infrastructure.Jobs;

public sealed class NotificationDispatchJob
{
    private readonly NotificationDbContext _db;
    private readonly IEmailService _emailService;
    private readonly ILogger<NotificationDispatchJob> _logger;

    public NotificationDispatchJob(
        NotificationDbContext db,
        IEmailService emailService,
        ILogger<NotificationDispatchJob> logger)
    {
        _db = db;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("NotificationDispatchJob: Processing pending notification queue...");

        var pending = await _db.NotificationLogs
            .Where(n => !n.IsSent && n.SentAt == null)
            .OrderBy(n => n.CreatedAt)
            .Take(50)
            .ToListAsync();

        if (pending.Count == 0)
        {
            _logger.LogInformation("NotificationDispatchJob: No pending notifications.");
            return;
        }

        _logger.LogInformation("NotificationDispatchJob: Found {Count} pending notifications.", pending.Count);

        foreach (var log in pending)
        {
            try
            {
                await _emailService.SendAsync(
                    log.Recipient,
                    log.Subject ?? "(no subject)",
                    string.Empty);

                _logger.LogInformation("NotificationDispatchJob: Sent notification {Id} to {Recipient}", log.Id, log.Recipient);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "NotificationDispatchJob: Failed to send notification {Id}", log.Id);
            }
        }
    }
}

