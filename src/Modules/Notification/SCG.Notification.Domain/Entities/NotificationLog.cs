using SCG.SharedKernel;

namespace SCG.Notification.Domain.Entities;

public sealed class NotificationLog : Entity<Guid>
{
    public string Channel { get; private set; } = default!; // Email, SMS
    public string Recipient { get; private set; } = default!;
    public string TemplateKey { get; private set; } = default!;
    public string? Subject { get; private set; }
    public bool IsSent { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime? SentAt { get; private set; }

    private NotificationLog() { } // EF

    public static NotificationLog Create(string channel, string recipient, string templateKey, string? subject)
    {
        return new NotificationLog
        {
            Id = Guid.NewGuid(),
            Channel = channel,
            Recipient = recipient,
            TemplateKey = templateKey,
            Subject = subject
        };
    }

    public void MarkSent()
    {
        IsSent = true;
        SentAt = DateTime.UtcNow;
    }

    public void MarkFailed(string errorMessage)
    {
        IsSent = false;
        ErrorMessage = errorMessage;
    }
}
