using SCG.SharedKernel;

namespace SCG.Rules.Domain.Entities;

public sealed class SubmissionWindow : Entity<Guid>
{
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly OpenTime { get; private set; }
    public TimeOnly CloseTime { get; private set; }
    public bool IsActive { get; private set; } = true;

    private SubmissionWindow() { } // EF

    public static SubmissionWindow Create(DayOfWeek dayOfWeek, TimeOnly openTime, TimeOnly closeTime)
    {
        return new SubmissionWindow
        {
            Id = Guid.NewGuid(),
            DayOfWeek = dayOfWeek,
            OpenTime = openTime,
            CloseTime = closeTime
        };
    }

    public void Update(TimeOnly openTime, TimeOnly closeTime)
    {
        OpenTime = openTime;
        CloseTime = closeTime;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public bool IsCurrentlyOpen()
    {
        var now = TimeOnly.FromDateTime(DateTime.UtcNow);
        return IsActive && DateTime.UtcNow.DayOfWeek == DayOfWeek && now >= OpenTime && now <= CloseTime;
    }
}
