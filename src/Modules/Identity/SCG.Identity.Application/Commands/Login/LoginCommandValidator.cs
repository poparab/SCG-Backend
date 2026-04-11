using FluentValidation;

namespace SCG.Identity.Application.Commands.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    private static readonly string[] ValidLoginTypes = ["agency", "admin", "individual"];

    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");

        RuleFor(x => x.LoginType)
            .NotEmpty().WithMessage("Login type is required.")
            .Must(lt => ValidLoginTypes.Contains(lt, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Login type must be one of: agency, admin, individual.");
    }
}
