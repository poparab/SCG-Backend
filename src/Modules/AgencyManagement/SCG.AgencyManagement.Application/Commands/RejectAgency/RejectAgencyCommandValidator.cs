using FluentValidation;

namespace SCG.AgencyManagement.Application.Commands.RejectAgency;

public sealed class RejectAgencyCommandValidator : AbstractValidator<RejectAgencyCommand>
{
    public RejectAgencyCommandValidator()
    {
        RuleFor(x => x.AgencyId)
            .NotEmpty().WithMessage("Agency ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Rejection reason is required.")
            .MaximumLength(1000).WithMessage("Rejection reason must not exceed 1000 characters.");
    }
}
