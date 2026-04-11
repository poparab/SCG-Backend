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
            .NotEmpty().WithMessage("Payment reference number is required.")
            .MaximumLength(100).WithMessage("Reference number must not exceed 100 characters.");
    }
}
