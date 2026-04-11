using FluentValidation;

namespace SCG.Rules.Application.Commands.ToggleNationalityInquiry;

public sealed class ToggleNationalityInquiryCommandValidator : AbstractValidator<ToggleNationalityInquiryCommand>
{
    public ToggleNationalityInquiryCommandValidator()
    {
        RuleFor(x => x.NationalityId)
            .NotEmpty().WithMessage("Nationality ID is required.");
    }
}
