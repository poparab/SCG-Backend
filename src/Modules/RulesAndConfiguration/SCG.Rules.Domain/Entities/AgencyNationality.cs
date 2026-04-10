using SCG.SharedKernel;

namespace SCG.Rules.Domain.Entities;

public sealed class AgencyNationality : Entity<Guid>
{
    public Guid AgencyId { get; private set; }
    public Guid NationalityId { get; private set; }
    public Nationality Nationality { get; private set; } = null!;
    public decimal? CustomFee { get; private set; }
    public bool IsEnabled { get; private set; }

    private AgencyNationality() { }

    public static AgencyNationality Create(Guid agencyId, Guid nationalityId, decimal? customFee = null)
    {
        return new AgencyNationality
        {
            Id = Guid.NewGuid(),
            AgencyId = agencyId,
            NationalityId = nationalityId,
            CustomFee = customFee,
            IsEnabled = true
        };
    }

    public void UpdateCustomFee(decimal? fee) => CustomFee = fee;
    public void Enable() => IsEnabled = true;
    public void Disable() => IsEnabled = false;
    public void ToggleEnabled() => IsEnabled = !IsEnabled;
}
