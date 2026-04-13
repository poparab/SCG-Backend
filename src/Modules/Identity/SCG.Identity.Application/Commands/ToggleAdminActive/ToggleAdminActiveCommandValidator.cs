using FluentValidation;

namespace SCG.Identity.Application.Commands.ToggleAdminActive;

public sealed class ToggleAdminActiveCommandValidator : AbstractValidator<ToggleAdminActiveCommand>
{
    public ToggleAdminActiveCommandValidator()
    {
        RuleFor(x => x.AdminUserId)
            .NotEmpty().WithMessage("Admin user ID is required.");
    }
}
