using FluentValidation;

namespace SCG.Rules.Application.Commands.UpdateNationalityFee;

public sealed class UpdateNationalityFeeCommandValidator : AbstractValidator<UpdateNationalityFeeCommand>
{
    public UpdateNationalityFeeCommandValidator()
    {
        RuleFor(x => x.NationalityId)
            .NotEmpty().WithMessage("Nationality ID is required.");

        RuleFor(x => x.NewFee)
            .GreaterThanOrEqualTo(0).WithMessage("Fee must not be negative.");

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty().WithMessage("Effective-from date is required.");

        RuleFor(x => x.EffectiveTo)
            .GreaterThan(x => x.EffectiveFrom).WithMessage("Effective-to date must be after effective-from date.")
            .When(x => x.EffectiveTo.HasValue);
    }
}
