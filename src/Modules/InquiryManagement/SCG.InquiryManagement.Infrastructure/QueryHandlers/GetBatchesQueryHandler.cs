using Microsoft.EntityFrameworkCore;
using SCG.Application.Abstractions.Messaging;
using SCG.InquiryManagement.Application.Queries.GetBatches;
using SCG.InquiryManagement.Infrastructure.Persistence;
using SCG.SharedKernel;

namespace SCG.InquiryManagement.Infrastructure.QueryHandlers;

internal sealed class GetBatchesQueryHandler : IQueryHandler<GetBatchesQuery, PagedResult<BatchListItemDto>>
{
    private readonly InquiryDbContext _db;

    public GetBatchesQueryHandler(InquiryDbContext db) => _db = db;

    public async Task<Result<PagedResult<BatchListItemDto>>> Handle(GetBatchesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Batches
            .Where(b => b.AgencyId == request.AgencyId)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            query = query.Where(b =>
                b.Name.ToLower().Contains(search));
        }

        if (request.Status.HasValue)
            query = query.Where(b => b.Status == request.Status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new BatchListItemDto(
                b.Id,
                b.Name,
                b.Status.ToString(),
                b.TravelerCount,
                b.CreatedAt,
                b.SubmittedAt))
            .ToListAsync(cancellationToken);

        return Result<PagedResult<BatchListItemDto>>.Success(
            new PagedResult<BatchListItemDto>(items, totalCount, request.Page, request.PageSize));
    }
}
