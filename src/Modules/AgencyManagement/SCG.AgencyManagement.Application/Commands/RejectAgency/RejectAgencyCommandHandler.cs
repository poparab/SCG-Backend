using SCG.AgencyManagement.Application.Abstractions;
using SCG.AgencyManagement.Domain.Enums;
using SCG.Application.Abstractions.Messaging;
using SCG.SharedKernel;

namespace SCG.AgencyManagement.Application.Commands.RejectAgency;

internal sealed class RejectAgencyCommandHandler : ICommandHandler<RejectAgencyCommand>
{
    private readonly IAgencyRepository _repository;

    public RejectAgencyCommandHandler(IAgencyRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(RejectAgencyCommand request, CancellationToken cancellationToken)
    {
        var agency = await _repository.GetByIdAsync(request.AgencyId, cancellationToken);
        if (agency is null)
            return Result.Failure("Agency not found.");

        if (agency.Status != AgencyStatus.PendingReview)
            return Result.Failure("Only agencies with Pending Review status can be rejected.");

        agency.Reject(request.Reason);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
