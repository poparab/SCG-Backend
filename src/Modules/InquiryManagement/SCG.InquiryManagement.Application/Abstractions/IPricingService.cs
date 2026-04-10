namespace SCG.InquiryManagement.Application.Abstractions;

public interface IPricingService
{
    Task<decimal?> GetFeeAsync(string nationalityCode, Guid inquiryTypeId, Guid? agencyId, CancellationToken ct = default);
}
