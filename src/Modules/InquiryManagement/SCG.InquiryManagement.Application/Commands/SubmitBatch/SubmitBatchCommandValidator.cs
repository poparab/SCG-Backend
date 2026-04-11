using FluentValidation;

namespace SCG.InquiryManagement.Application.Commands.SubmitBatch;

public sealed class SubmitBatchCommandValidator : AbstractValidator<SubmitBatchCommand>
{
    public SubmitBatchCommandValidator()
    {
        RuleFor(x => x.BatchId)
            .NotEmpty().WithMessage("Batch ID is required.");

        RuleFor(x => x.SubmittedByUserId)
            .NotEmpty().WithMessage("Submitted-by user ID is required.");
    }
}
