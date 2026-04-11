using FluentValidation;

namespace SCG.InquiryManagement.Application.Commands.CreateBatch;

public sealed class CreateBatchCommandValidator : AbstractValidator<CreateBatchCommand>
{
    public CreateBatchCommandValidator()
    {
        RuleFor(x => x.AgencyId)
            .NotEmpty().WithMessage("Agency ID is required.");

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty().WithMessage("Created-by user ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Batch name is required.")
            .MaximumLength(200).WithMessage("Batch name must not exceed 200 characters.");

        RuleFor(x => x.InquiryTypeId)
            .NotEmpty().WithMessage("Inquiry type is required.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters.")
            .When(x => x.Notes is not null);
    }
}
