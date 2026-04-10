using Microsoft.EntityFrameworkCore;
using SCG.AgencyManagement.Infrastructure.Persistence;
using SCG.Application.Abstractions.Messaging;
using SCG.InquiryManagement.Infrastructure.Persistence;
using SCG.SharedKernel;

namespace SCG.API.Queries.GetAgencyById;

internal sealed class GetAgencyByIdQueryHandler : IQueryHandler<GetAgencyByIdQuery, AgencyDetailDto>
{
    private readonly AgencyDbContext _agencyDb;
    private readonly InquiryDbContext _inquiryDb;

    public GetAgencyByIdQueryHandler(AgencyDbContext agencyDb, InquiryDbContext inquiryDb)
    {
        _agencyDb = agencyDb;
        _inquiryDb = inquiryDb;
    }

    public async Task<Result<AgencyDetailDto>> Handle(GetAgencyByIdQuery request, CancellationToken cancellationToken)
    {
        var agency = await _agencyDb.Agencies
            .Include(a => a.Wallet)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (agency is null)
            return Result<AgencyDetailDto>.Failure("Agency not found.");

        var totalBatches = await _inquiryDb.Batches
            .CountAsync(b => b.AgencyId == request.Id, cancellationToken);

        var totalInquiries = await _inquiryDb.Inquiries
            .CountAsync(i => i.AgencyId == request.Id, cancellationToken);

        return Result<AgencyDetailDto>.Success(new AgencyDetailDto(
            agency.Id,
            agency.NameEn,
            agency.NameAr,
            agency.Email,
            agency.Phone,
            agency.CommercialLicenseNumber,
            agency.CommercialLicenseExpiry,
            agency.Address,
            agency.Status,
            agency.RejectionReason,
            agency.Wallet?.Balance ?? 0m,
            agency.Wallet?.Currency ?? "USD",
            totalBatches,
            totalInquiries,
            agency.CreatedAt));
    }
}
