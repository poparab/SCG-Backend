using SCG.SharedKernel;

namespace SCG.Rules.Domain.Entities;

public sealed class SystemAnnouncement : Entity<Guid>
{
    public string TitleAr { get; private set; } = default!;
    public string TitleEn { get; private set; } = default!;
    public string MessageAr { get; private set; } = default!;
    public string MessageEn { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public string Severity { get; private set; } = "info"; // info, warning, danger

    private SystemAnnouncement() { } // EF

    public static SystemAnnouncement Create(
        string titleAr, string titleEn,
        string messageAr, string messageEn,
        DateTime? startDate, DateTime? endDate,
        string severity = "info")
    {
        return new SystemAnnouncement
        {
            Id = Guid.NewGuid(),
            TitleAr = titleAr,
            TitleEn = titleEn,
            MessageAr = messageAr,
            MessageEn = messageEn,
            StartDate = startDate,
            EndDate = endDate,
            Severity = severity
        };
    }

    public void Update(string titleAr, string titleEn, string messageAr, string messageEn,
        DateTime? startDate, DateTime? endDate, string severity)
    {
        TitleAr = titleAr;
        TitleEn = titleEn;
        MessageAr = messageAr;
        MessageEn = messageEn;
        StartDate = startDate;
        EndDate = endDate;
        Severity = severity;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
