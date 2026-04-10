using SCG.Application.Abstractions.Messaging;
using SCG.InquiryManagement.Application.Abstractions;
using SCG.InquiryManagement.Domain.Enums;
using SCG.SharedKernel;

namespace SCG.InquiryManagement.Application.Commands.UpdateTravelerInBatch;

internal sealed class UpdateTravelerInBatchCommandHandler : ICommandHandler<UpdateTravelerInBatchCommand>
{
    private readonly IBatchRepository _batchRepository;

    public UpdateTravelerInBatchCommandHandler(IBatchRepository batchRepository)
    {
        _batchRepository = batchRepository;
    }

    public async Task<Result> Handle(UpdateTravelerInBatchCommand request, CancellationToken cancellationToken)
    {
        var batch = await _batchRepository.GetByIdWithTravelersAsync(request.BatchId, cancellationToken);

        if (batch is null)
            return Result.Failure("Batch not found.");

        if (batch.Status != BatchStatus.Draft)
            return Result.Failure("Travelers can only be updated in batches with Draft status.");

        var traveler = batch.Travelers.FirstOrDefault(t => t.Id == request.TravelerId);
        if (traveler is null)
            return Result.Failure("Traveler not found in this batch.");

        traveler.Update(
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

        await _batchRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
