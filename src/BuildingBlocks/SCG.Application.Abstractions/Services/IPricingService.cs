namespace SCG.Application.Abstractions.Services;

public interface IPricingService
{
    Task<decimal?> GetFeeAsync(string nationalityCode, Guid inquiryTypeId, Guid? agencyId, CancellationToken ct = default);
}
