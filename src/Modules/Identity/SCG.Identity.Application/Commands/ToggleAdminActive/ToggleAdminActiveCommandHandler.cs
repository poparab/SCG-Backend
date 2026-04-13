using SCG.Application.Abstractions.Messaging;
using SCG.Identity.Application.Abstractions;
using SCG.SharedKernel;

namespace SCG.Identity.Application.Commands.ToggleAdminActive;

public sealed class ToggleAdminActiveCommandHandler : ICommandHandler<ToggleAdminActiveCommand>
{
    private readonly IAdminUserRepository _repository;

    public ToggleAdminActiveCommandHandler(IAdminUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(ToggleAdminActiveCommand request, CancellationToken cancellationToken)
    {
        var admin = await _repository.GetByIdAsync(request.AdminUserId, cancellationToken);
        if (admin is null)
            return Result.Failure("Admin user not found.");

        if (admin.IsActive)
            admin.Deactivate();
        else
            admin.Activate();

        await _repository.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
