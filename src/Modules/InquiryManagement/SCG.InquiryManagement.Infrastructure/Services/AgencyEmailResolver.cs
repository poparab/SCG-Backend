using Microsoft.EntityFrameworkCore;
using SCG.InquiryManagement.Application.Abstractions;
using SCG.InquiryManagement.Infrastructure.Persistence;

namespace SCG.InquiryManagement.Infrastructure.Services;

internal sealed class AgencyEmailResolver : IAgencyEmailResolver
{
    private readonly InquiryDbContext _db;

    public AgencyEmailResolver(InquiryDbContext db) => _db = db;

    public async Task<string?> GetAgencyEmailAsync(Guid agencyId, CancellationToken ct = default)
    {
        var results = await _db.Database
            .SqlQuery<string>($"SELECT Email FROM [agency].[Agencies] WHERE Id = {agencyId}")
            .ToListAsync(ct);

        return results.FirstOrDefault();
    }
}
