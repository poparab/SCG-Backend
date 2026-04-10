using SCG.Application.Abstractions.Messaging;
using SCG.InquiryManagement.Domain.Enums;
using SCG.SharedKernel;

namespace SCG.InquiryManagement.Application.Queries.GetInquiries;

public sealed record GetInquiriesQuery(
    Guid? AgencyId,
    string? SearchTerm,
    InquiryStatus? Status,
    string? NationalityCode,
    DateTime? DateFrom,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<InquiryListItemDto>>;

public sealed record InquiryListItemDto(
    Guid Id,
    string ReferenceNumber,
    string FirstNameEn,
    string LastNameEn,
    string PassportNumber,
    string NationalityCode,
    string Status,
    DateTime TravelDate,
    DateTime CreatedAt);
