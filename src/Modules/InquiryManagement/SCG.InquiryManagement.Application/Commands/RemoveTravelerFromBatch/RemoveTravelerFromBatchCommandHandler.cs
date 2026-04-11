using SCG.Application.Abstractions.Messaging;
using SCG.InquiryManagement.Application.Abstractions;
using SCG.InquiryManagement.Domain.Enums;
using SCG.SharedKernel;

namespace SCG.InquiryManagement.Application.Commands.RemoveTravelerFromBatch;

public sealed class RemoveTravelerFromBatchCommandHandler : ICommandHandler<RemoveTravelerFromBatchCommand>
{
    private readonly IBatchRepository _batchRepository;

    public RemoveTravelerFromBatchCommandHandler(IBatchRepository batchRepository)
    {
        _batchRepository = batchRepository;
    }

    public async Task<Result> Handle(RemoveTravelerFromBatchCommand request, CancellationToken cancellationToken)
    {
        var batch = await _batchRepository.GetByIdWithTravelersAsync(request.BatchId, cancellationToken);

        if (batch is null)
            return Result.Failure("Batch not found.");

        if (batch.Status != BatchStatus.Draft)
            return Result.Failure("Travelers can only be removed from batches in Draft status.");

        var traveler = batch.Travelers.FirstOrDefault(t => t.Id == request.TravelerId);
        if (traveler is null)
            return Result.Failure("Traveler not found in this batch.");

        batch.RemoveTraveler(request.TravelerId);
        await _batchRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
