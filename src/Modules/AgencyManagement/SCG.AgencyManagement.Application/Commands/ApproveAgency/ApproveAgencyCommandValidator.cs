using FluentValidation;

namespace SCG.AgencyManagement.Application.Commands.ApproveAgency;

public sealed class ApproveAgencyCommandValidator : AbstractValidator<ApproveAgencyCommand>
{
    public ApproveAgencyCommandValidator()
    {
        RuleFor(x => x.AgencyId)
            .NotEmpty().WithMessage("Agency ID is required.");
    }
}
