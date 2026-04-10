using SCG.Application.Abstractions.Messaging;
using SCG.InquiryManagement.Application.Abstractions;
using SCG.InquiryManagement.Domain.Entities;
using SCG.InquiryManagement.Domain.Enums;
using SCG.SharedKernel;

namespace SCG.InquiryManagement.Application.Commands.AddTravelerToBatch;

internal sealed class AddTravelerToBatchCommandHandler : ICommandHandler<AddTravelerToBatchCommand, Guid>
{
    private readonly IBatchRepository _batchRepository;

    public AddTravelerToBatchCommandHandler(IBatchRepository batchRepository)
    {
        _batchRepository = batchRepository;
    }

    public async Task<Result<Guid>> Handle(AddTravelerToBatchCommand request, CancellationToken cancellationToken)
    {
        var batch = await _batchRepository.GetByIdWithTravelersAsync(request.BatchId, cancellationToken);

        if (batch is null)
            return Result<Guid>.Failure("Batch not found.");

        if (batch.Status != BatchStatus.Draft)
            return Result<Guid>.Failure("Travelers can only be added to batches in Draft status.");

        var nextRowIndex = batch.Travelers.Count > 0
            ? batch.Travelers.Max(t => t.RowIndex) + 1
            : 1;

        var traveler = BatchTraveler.Create(
            batch.Id,
            nextRowIndex,
            request.FirstNameEn,
            request.LastNameEn,
            request.FirstNameAr,
            request.LastNameAr,
            request.PassportNumber,
            request.NationalityCode,
            request.DateOfBirth,
            request.Gender,
            request.TravelDate,
            request.ArrivalAirport,
            request.TransitCountries,
            request.PassportExpiry,
            request.DepartureCountry,
            request.PurposeOfTravel,
            request.FlightNumber);

        batch.AddTraveler(traveler);
        await _batchRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(traveler.Id);
    }
}
