using FluentValidation;

namespace SCG.InquiryManagement.Application.Commands.RejectInquiry;

public sealed class RejectInquiryCommandValidator : AbstractValidator<RejectInquiryCommand>
{
    public RejectInquiryCommandValidator()
    {
        RuleFor(x => x.InquiryId)
            .NotEmpty().WithMessage("Inquiry ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Rejection reason is required.")
            .MinimumLength(10).WithMessage("Rejection reason must be at least 10 characters.");
    }
}
