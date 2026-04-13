namespace SCG.InquiryManagement.Application.Abstractions;

public interface IAgencyEmailResolver
{
    Task<string?> GetAgencyEmailAsync(Guid agencyId, CancellationToken ct = default);
}
