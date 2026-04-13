using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SCG.Notification.Application.Services;
using SCG.Notification.Domain.Entities;
using SCG.Notification.Infrastructure.Configuration;
using SCG.Notification.Infrastructure.Persistence;
using System.Net.Mail;

namespace SCG.Notification.Infrastructure.Services;

internal sealed class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly NotificationDbContext _db;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(
        IOptions<SmtpSettings> settings,
        NotificationDbContext db,
        ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _db = db;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        var log = NotificationLog.Create("Email", to, "generic", subject);
        await _db.NotificationLogs.AddAsync(log, ct);

        if (!_settings.IsEnabled || string.IsNullOrWhiteSpace(_settings.Host))
        {
            _logger.LogWarning("SMTP is disabled or not configured. Skipping send to {Recipient}", to);
            await _db.SaveChangesAsync(ct);
            return;
        }

        try
        {
            using var smtp = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new System.Net.NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = _settings.EnableSsl
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(_settings.FromAddress, _settings.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            mail.To.Add(to);
            await smtp.SendMailAsync(mail, ct);
            log.MarkSent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient}", to);
            log.MarkFailed(ex.Message);
        }

        await _db.SaveChangesAsync(ct);
    }
}
