using FluentValidation;

namespace SCG.API.Commands.PropagateNationalityToAgencies;

public sealed class PropagateNationalityToAgenciesCommandValidator : AbstractValidator<PropagateNationalityToAgenciesCommand>
{
    public PropagateNationalityToAgenciesCommandValidator()
    {
        RuleFor(x => x.NationalityId)
            .NotEmpty().WithMessage("Nationality ID is required.");
    }
}
