using SCG.Application.Abstractions.Messaging;
using SCG.InquiryManagement.Application.Abstractions;
using SCG.InquiryManagement.Domain.Enums;
using SCG.SharedKernel;

namespace SCG.InquiryManagement.Application.Commands.ApproveInquiry;

public sealed class ApproveInquiryCommandHandler : ICommandHandler<ApproveInquiryCommand>
{
    private readonly IInquiryRepository _repository;

    public ApproveInquiryCommandHandler(IInquiryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(ApproveInquiryCommand request, CancellationToken cancellationToken)
    {
        var inquiry = await _repository.GetByIdAsync(request.InquiryId, cancellationToken);
        if (inquiry is null)
            return Result.Failure("Inquiry not found.");

        if (inquiry.Status != InquiryStatus.Submitted && inquiry.Status != InquiryStatus.UnderProcessing)
            return Result.Failure("Only inquiries in Submitted or Under Processing status can be approved.");

        inquiry.Approve(documentUrl: null);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
