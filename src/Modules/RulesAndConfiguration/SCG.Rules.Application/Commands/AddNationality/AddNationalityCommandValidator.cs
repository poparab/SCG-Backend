using FluentValidation;

namespace SCG.Rules.Application.Commands.AddNationality;

public sealed class AddNationalityCommandValidator : AbstractValidator<AddNationalityCommand>
{
    public AddNationalityCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Nationality code is required.")
            .Length(2, 3).WithMessage("Nationality code must be 2 or 3 characters.")
            .Matches(@"^[A-Za-z]+$").WithMessage("Nationality code must contain only letters.");

        RuleFor(x => x.NameAr)
            .NotEmpty().WithMessage("Arabic name is required.")
            .MaximumLength(200).WithMessage("Arabic name must not exceed 200 characters.");

        RuleFor(x => x.NameEn)
            .NotEmpty().WithMessage("English name is required.")
            .MaximumLength(200).WithMessage("English name must not exceed 200 characters.");

        RuleFor(x => x.DefaultFee)
            .GreaterThanOrEqualTo(0).WithMessage("Default fee must not be negative.");
    }
}
