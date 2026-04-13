using SCG.Application.Abstractions.Messaging;
using SCG.Identity.Application.Abstractions;
using SCG.SharedKernel;

namespace SCG.Identity.Application.Commands.UpdateAdminUser;

public sealed class UpdateAdminUserCommandHandler : ICommandHandler<UpdateAdminUserCommand>
{
    private readonly IAdminUserRepository _repository;

    public UpdateAdminUserCommandHandler(IAdminUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(UpdateAdminUserCommand request, CancellationToken cancellationToken)
    {
        var admin = await _repository.GetByIdAsync(request.AdminUserId, cancellationToken);
        if (admin is null)
            return Result.Failure("Admin user not found.");

        admin.UpdateProfile(request.FullName);
        admin.UpdateRole(request.Role);

        await _repository.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
