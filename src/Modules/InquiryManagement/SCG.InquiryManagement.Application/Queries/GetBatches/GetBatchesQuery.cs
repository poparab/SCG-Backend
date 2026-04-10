using SCG.Application.Abstractions.Messaging;
using SCG.InquiryManagement.Domain.Enums;
using SCG.SharedKernel;

namespace SCG.InquiryManagement.Application.Queries.GetBatches;

public sealed record GetBatchesQuery(
    Guid AgencyId,
    string? SearchTerm,
    BatchStatus? Status,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<BatchListItemDto>>;

public sealed record BatchListItemDto(
    Guid Id,
    string Name,
    string Status,
    int TravelerCount,
    DateTime CreatedAt,
    DateTime? SubmittedAt);
