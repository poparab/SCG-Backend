using Microsoft.EntityFrameworkCore;
using SCG.Application.Abstractions.Messaging;
using SCG.InquiryManagement.Application.Queries.GetBatchById;
using SCG.InquiryManagement.Infrastructure.Persistence;
using SCG.SharedKernel;

namespace SCG.InquiryManagement.Infrastructure.QueryHandlers;

internal sealed class GetBatchByIdQueryHandler : IQueryHandler<GetBatchByIdQuery, BatchDetailDto>
{
    private readonly InquiryDbContext _db;

    public GetBatchByIdQueryHandler(InquiryDbContext db) => _db = db;

    public async Task<Result<BatchDetailDto>> Handle(GetBatchByIdQuery request, CancellationToken cancellationToken)
    {
        var batch = await _db.Batches
            .Include(b => b.Travelers)
                .ThenInclude(t => t.Inquiry)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.BatchId, cancellationToken);

        if (batch is null)
            return Result<BatchDetailDto>.Failure("Batch not found.");

        var travelers = batch.Travelers
            .OrderBy(t => t.RowIndex)
            .Select(t => new BatchTravelerDto(
                t.Id,
                t.FirstNameEn,
                t.LastNameEn,
                t.FirstNameAr,
                t.LastNameAr,
                t.PassportNumber,
                t.NationalityCode,
                t.DateOfBirth,
                t.Gender.ToString(),
                t.TravelDate,
                t.ArrivalAirport,
                t.PassportExpiry,
                t.DepartureCountry,
                t.PurposeOfTravel,
                t.FlightNumber,
                t.InquiryId,
                t.Inquiry?.Status.ToString(),
                t.Inquiry?.ReferenceNumber))
            .ToList();

        return Result<BatchDetailDto>.Success(new BatchDetailDto(
            batch.Id,
            batch.Name,
            batch.Notes,
            batch.Status.ToString(),
            batch.TravelerCount,
            batch.CreatedAt,
            batch.SubmittedAt,
            batch.TotalFee,
            batch.PaymentReference,
            travelers));
    }
}
