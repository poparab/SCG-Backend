using SCG.Application.Abstractions.Messaging;
using SCG.Rules.Application.Abstractions;
using SCG.Rules.Domain.Entities;
using SCG.SharedKernel;

namespace SCG.Rules.Application.Commands.AddNationality;

public sealed class AddNationalityCommandHandler : ICommandHandler<AddNationalityCommand, Guid>
{
    private readonly INationalityRepository _repository;

    public AddNationalityCommandHandler(INationalityRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(AddNationalityCommand request, CancellationToken cancellationToken)
    {
        var exists = await _repository.ExistsByCodeAsync(request.Code, cancellationToken);
        if (exists)
            return Result<Guid>.Failure("Nationality with this code already exists.");

        var nationality = Nationality.Create(
            request.Code.ToUpperInvariant(),
            request.NameAr,
            request.NameEn,
            request.RequiresInquiry);

        await _repository.AddAsync(nationality, cancellationToken);

        if (request.RequiresInquiry && request.DefaultFee > 0)
        {
            var inquiryTypeId = await _repository.GetDefaultInquiryTypeIdAsync(cancellationToken);

            nationality.AddInquiryType(inquiryTypeId);

            var pricing = Pricing.Create(
                inquiryTypeId,
                request.DefaultFee,
                DateTime.UtcNow,
                effectiveTo: null,
                nationalityCode: nationality.Code);

            await _repository.AddPricingAsync(pricing, cancellationToken);
        }

        await _repository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(nationality.Id);
    }
}
