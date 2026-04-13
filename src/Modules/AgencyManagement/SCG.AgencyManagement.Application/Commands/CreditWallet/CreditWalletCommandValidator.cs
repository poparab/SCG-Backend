using FluentValidation;

namespace SCG.AgencyManagement.Application.Commands.CreditWallet;

public sealed class CreditWalletCommandValidator : AbstractValidator<CreditWalletCommand>
{
    public CreditWalletCommandValidator()
    {
        RuleFor(x => x.AgencyId)
            .NotEmpty().WithMessage("Agency ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Credit amount must be greater than zero.");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Payment method is required.")
            .MaximumLength(50).WithMessage("Payment method must not exceed 50 characters.");

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(50).WithMessage("Reference number must not exceed 50 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.ReferenceNumber));
    }
}
