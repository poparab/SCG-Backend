using SCG.Application.Abstractions.Messaging;
using SCG.Rules.Application.Abstractions;
using SCG.SharedKernel;

namespace SCG.Rules.Application.Commands.ToggleNationalityInquiry;

internal sealed class ToggleNationalityInquiryCommandHandler : ICommandHandler<ToggleNationalityInquiryCommand>
{
    private readonly INationalityRepository _repository;

    public ToggleNationalityInquiryCommandHandler(INationalityRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(ToggleNationalityInquiryCommand request, CancellationToken cancellationToken)
    {
        var nationality = await _repository.GetByIdAsync(request.NationalityId, cancellationToken);
        if (nationality is null)
            return Result.Failure("Nationality not found.");

        nationality.ToggleInquiryRequirement(request.RequiresInquiry);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
