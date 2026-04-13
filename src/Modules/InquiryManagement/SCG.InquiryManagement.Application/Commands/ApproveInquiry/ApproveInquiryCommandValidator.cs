using FluentValidation;

namespace SCG.InquiryManagement.Application.Commands.ApproveInquiry;

public sealed class ApproveInquiryCommandValidator : AbstractValidator<ApproveInquiryCommand>
{
    public ApproveInquiryCommandValidator()
    {
        RuleFor(x => x.InquiryId)
            .NotEmpty().WithMessage("Inquiry ID is required.");
    }
}
