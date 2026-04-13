namespace SCG.Notification.Infrastructure.Configuration;

public sealed class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "Security Clearance System";
    public bool EnableSsl { get; set; } = true;
    public bool IsEnabled { get; set; } = false;
}
