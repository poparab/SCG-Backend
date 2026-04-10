using Microsoft.EntityFrameworkCore;
using SCG.Application.Abstractions.Messaging;
using SCG.InquiryManagement.Application.Queries.GetInquiries;
using SCG.InquiryManagement.Infrastructure.Persistence;
using SCG.SharedKernel;

namespace SCG.InquiryManagement.Infrastructure.QueryHandlers;

internal sealed class GetInquiriesQueryHandler : IQueryHandler<GetInquiriesQuery, PagedResult<InquiryListItemDto>>
{
    private readonly InquiryDbContext _db;

    public GetInquiriesQueryHandler(InquiryDbContext db) => _db = db;

    public async Task<Result<PagedResult<InquiryListItemDto>>> Handle(GetInquiriesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Inquiries.AsNoTracking();

        if (request.AgencyId.HasValue)
            query = query.Where(i => i.AgencyId == request.AgencyId.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            query = query.Where(i =>
                i.ReferenceNumber.ToLower().Contains(search) ||
                i.FirstNameEn.ToLower().Contains(search) ||
                i.LastNameEn.ToLower().Contains(search) ||
                i.PassportNumber.ToLower().Contains(search));
        }

        if (request.Status.HasValue)
            query = query.Where(i => i.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.NationalityCode))
            query = query.Where(i => i.NationalityCode == request.NationalityCode);

        if (request.DateFrom.HasValue)
            query = query.Where(i => i.CreatedAt.Date == request.DateFrom.Value.Date);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new InquiryListItemDto(
                i.Id,
                i.ReferenceNumber,
                i.FirstNameEn,
                i.LastNameEn,
                i.PassportNumber,
                i.NationalityCode,
                i.Status.ToString(),
                i.TravelDate,
                i.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<PagedResult<InquiryListItemDto>>.Success(
            new PagedResult<InquiryListItemDto>(items, totalCount, request.Page, request.PageSize));
    }
}
