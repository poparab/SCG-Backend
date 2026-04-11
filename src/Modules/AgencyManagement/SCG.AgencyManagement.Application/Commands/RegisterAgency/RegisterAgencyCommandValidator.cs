using FluentValidation;

namespace SCG.AgencyManagement.Application.Commands.RegisterAgency;

public sealed class RegisterAgencyCommandValidator : AbstractValidator<RegisterAgencyCommand>
{
    public RegisterAgencyCommandValidator()
    {
        RuleFor(x => x.AgencyName)
            .NotEmpty().WithMessage("Agency name is required.")
            .MaximumLength(200).WithMessage("Agency name must not exceed 200 characters.");

        RuleFor(x => x.ContactPersonName)
            .NotEmpty().WithMessage("Contact person name is required.")
            .MaximumLength(200).WithMessage("Contact person name must not exceed 200 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

        RuleFor(x => x.CountryCode)
            .NotEmpty().WithMessage("Country code is required.")
            .MaximumLength(5).WithMessage("Country code must not exceed 5 characters.");

        RuleFor(x => x.MobileNumber)
            .NotEmpty().WithMessage("Mobile number is required.")
            .Matches(@"^\d{7,15}$").WithMessage("Mobile number must be 7–15 digits.");
    }
}
