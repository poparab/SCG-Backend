using FluentValidation;

namespace SCG.InquiryManagement.Application.Commands.RemoveTravelerFromBatch;

public sealed class RemoveTravelerFromBatchCommandValidator : AbstractValidator<RemoveTravelerFromBatchCommand>
{
    public RemoveTravelerFromBatchCommandValidator()
    {
        RuleFor(x => x.BatchId)
            .NotEmpty().WithMessage("Batch ID is required.");

        RuleFor(x => x.TravelerId)
            .NotEmpty().WithMessage("Traveler ID is required.");
    }
}
