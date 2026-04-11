using FluentValidation;

namespace SCG.API.Commands.PropagateNationalitiesToAgency;

public sealed class PropagateNationalitiesToAgencyCommandValidator : AbstractValidator<PropagateNationalitiesToAgencyCommand>
{
    public PropagateNationalitiesToAgencyCommandValidator()
    {
        RuleFor(x => x.AgencyId)
            .NotEmpty().WithMessage("Agency ID is required.");
    }
}
