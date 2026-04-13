using SCG.Application.Abstractions.Messaging;
using SCG.InquiryManagement.Application.Abstractions;
using SCG.InquiryManagement.Domain.Enums;
using SCG.SharedKernel;

namespace SCG.InquiryManagement.Application.Commands.RejectInquiry;

public sealed class RejectInquiryCommandHandler : ICommandHandler<RejectInquiryCommand>
{
    private readonly IInquiryRepository _repository;

    public RejectInquiryCommandHandler(IInquiryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(RejectInquiryCommand request, CancellationToken cancellationToken)
    {
        var inquiry = await _repository.GetByIdAsync(request.InquiryId, cancellationToken);
        if (inquiry is null)
            return Result.Failure("Inquiry not found.");

        if (inquiry.Status != InquiryStatus.Submitted && inquiry.Status != InquiryStatus.UnderProcessing)
            return Result.Failure("Only inquiries in Submitted or Under Processing status can be rejected.");

        inquiry.Reject(request.Reason);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
