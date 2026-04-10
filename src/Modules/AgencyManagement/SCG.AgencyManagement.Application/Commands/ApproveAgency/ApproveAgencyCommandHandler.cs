using SCG.AgencyManagement.Application.Abstractions;
using SCG.AgencyManagement.Domain.Enums;
using SCG.Application.Abstractions.Messaging;
using SCG.SharedKernel;

namespace SCG.AgencyManagement.Application.Commands.ApproveAgency;

internal sealed class ApproveAgencyCommandHandler : ICommandHandler<ApproveAgencyCommand>
{
    private readonly IAgencyRepository _repository;

    public ApproveAgencyCommandHandler(IAgencyRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(ApproveAgencyCommand request, CancellationToken cancellationToken)
    {
        var agency = await _repository.GetByIdAsync(request.AgencyId, cancellationToken);
        if (agency is null)
            return Result.Failure("Agency not found.");

        if (agency.Status != AgencyStatus.PendingReview)
            return Result.Failure("Only agencies with Pending Review status can be approved.");

        agency.Approve();
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
