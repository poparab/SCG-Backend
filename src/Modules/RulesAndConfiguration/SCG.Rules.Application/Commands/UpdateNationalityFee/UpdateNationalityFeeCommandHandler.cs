using SCG.Application.Abstractions.Messaging;
using SCG.Rules.Application.Abstractions;
using SCG.Rules.Domain.Entities;
using SCG.SharedKernel;

namespace SCG.Rules.Application.Commands.UpdateNationalityFee;

public sealed class UpdateNationalityFeeCommandHandler : ICommandHandler<UpdateNationalityFeeCommand>
{
    private readonly INationalityRepository _repository;

    public UpdateNationalityFeeCommandHandler(INationalityRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(UpdateNationalityFeeCommand request, CancellationToken cancellationToken)
    {
        var nationality = await _repository.GetByIdAsync(request.NationalityId, cancellationToken);
        if (nationality is null)
            return Result.Failure("Nationality not found.");

        var inquiryTypeId = await _repository.GetDefaultInquiryTypeIdAsync(cancellationToken);

        var activePricing = await _repository.GetActivePricingForNationalityAsync(
            nationality.Code, inquiryTypeId, cancellationToken);

        if (activePricing is not null)
        {
            activePricing.Deactivate();
        }

        var newPricing = Pricing.Create(
            inquiryTypeId,
            request.NewFee,
            request.EffectiveFrom,
            request.EffectiveTo,
            nationalityCode: nationality.Code);

        await _repository.AddPricingAsync(newPricing, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
