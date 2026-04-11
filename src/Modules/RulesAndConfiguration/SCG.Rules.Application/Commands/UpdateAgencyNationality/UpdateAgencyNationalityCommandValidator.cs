using FluentValidation;

namespace SCG.Rules.Application.Commands.UpdateAgencyNationality;

public sealed class UpdateAgencyNationalityCommandValidator : AbstractValidator<UpdateAgencyNationalityCommand>
{
    public UpdateAgencyNationalityCommandValidator()
    {
        RuleFor(x => x.AgencyId)
            .NotEmpty().WithMessage("Agency ID is required.");

        RuleFor(x => x.NationalityId)
            .NotEmpty().WithMessage("Nationality ID is required.");

        RuleFor(x => x.CustomFee)
            .GreaterThanOrEqualTo(0).WithMessage("Custom fee must not be negative.")
            .When(x => x.CustomFee.HasValue);
    }
}
