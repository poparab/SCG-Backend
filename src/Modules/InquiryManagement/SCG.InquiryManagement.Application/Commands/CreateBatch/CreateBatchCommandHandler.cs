using SCG.Application.Abstractions.Messaging;
using SCG.InquiryManagement.Application.Abstractions;
using SCG.InquiryManagement.Domain.Entities;
using SCG.SharedKernel;

namespace SCG.InquiryManagement.Application.Commands.CreateBatch;

public sealed class CreateBatchCommandHandler : ICommandHandler<CreateBatchCommand, Guid>
{
    private readonly IBatchRepository _batchRepository;

    public CreateBatchCommandHandler(IBatchRepository batchRepository)
    {
        _batchRepository = batchRepository;
    }

    public async Task<Result<Guid>> Handle(CreateBatchCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<Guid>.Failure("Batch name is required.");

        var batch = Batch.Create(
            request.AgencyId,
            request.Name.Trim(),
            request.InquiryTypeId,
            request.Notes?.Trim(),
            request.CreatedByUserId);

        await _batchRepository.AddAsync(batch, cancellationToken);
        await _batchRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(batch.Id);
    }
}
