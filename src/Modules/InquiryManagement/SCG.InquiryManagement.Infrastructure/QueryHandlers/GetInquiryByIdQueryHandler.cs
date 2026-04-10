using Microsoft.EntityFrameworkCore;
using SCG.Application.Abstractions.Messaging;
using SCG.InquiryManagement.Application.Queries.GetInquiryById;
using SCG.InquiryManagement.Infrastructure.Persistence;
using SCG.SharedKernel;

namespace SCG.InquiryManagement.Infrastructure.QueryHandlers;

internal sealed class GetInquiryByIdQueryHandler : IQueryHandler<GetInquiryByIdQuery, InquiryDetailDto>
{
    private readonly InquiryDbContext _db;

    public GetInquiryByIdQueryHandler(InquiryDbContext db) => _db = db;

    public async Task<Result<InquiryDetailDto>> Handle(GetInquiryByIdQuery request, CancellationToken cancellationToken)
    {
        var inquiry = await _db.Inquiries
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == request.InquiryId, cancellationToken);

        if (inquiry is null)
            return Result<InquiryDetailDto>.Failure("Inquiry not found.");

        return Result<InquiryDetailDto>.Success(new InquiryDetailDto(
            inquiry.Id,
            inquiry.ReferenceNumber,
            inquiry.FirstNameEn,
            inquiry.LastNameEn,
            inquiry.FirstNameAr,
            inquiry.LastNameAr,
            inquiry.PassportNumber,
            inquiry.NationalityCode,
            inquiry.DateOfBirth,
            inquiry.Gender.ToString(),
            inquiry.TravelDate,
            inquiry.ArrivalAirport,
            inquiry.TransitCountries,
            inquiry.Status.ToString(),
            inquiry.Fee,
            inquiry.PaymentReference,
            inquiry.ResultCode,
            inquiry.RejectionReason,
            inquiry.ProcessedAt,
            inquiry.DocumentUrl,
            inquiry.BatchId,
            inquiry.AgencyId,
            inquiry.CreatedAt));
    }
}
