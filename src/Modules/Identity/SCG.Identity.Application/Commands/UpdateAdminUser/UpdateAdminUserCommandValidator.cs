using FluentValidation;

namespace SCG.Identity.Application.Commands.UpdateAdminUser;

public sealed class UpdateAdminUserCommandValidator : AbstractValidator<UpdateAdminUserCommand>
{
    private static readonly string[] AllowedRoles = ["Admin", "SuperAdmin"];

    public UpdateAdminUserCommandValidator()
    {
        RuleFor(x => x.AdminUserId)
            .NotEmpty().WithMessage("Admin user ID is required.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(200).WithMessage("Full name must not exceed 200 characters.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(r => AllowedRoles.Contains(r, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Role must be Admin or SuperAdmin.");
    }
}
