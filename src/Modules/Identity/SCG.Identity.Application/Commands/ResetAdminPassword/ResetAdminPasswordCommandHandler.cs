using SCG.Application.Abstractions.Messaging;
using SCG.Identity.Application.Abstractions;
using SCG.Identity.Application.Services;
using SCG.SharedKernel;

namespace SCG.Identity.Application.Commands.ResetAdminPassword;

public sealed class ResetAdminPasswordCommandHandler : ICommandHandler<ResetAdminPasswordCommand>
{
    private readonly IAdminUserRepository _repository;
    private readonly IPasswordHasher _passwordHasher;

    public ResetAdminPasswordCommandHandler(
        IAdminUserRepository repository,
        IPasswordHasher passwordHasher)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result> Handle(ResetAdminPasswordCommand request, CancellationToken cancellationToken)
    {
        var admin = await _repository.GetByIdAsync(request.AdminUserId, cancellationToken);
        if (admin is null)
            return Result.Failure("Admin user not found.");

        var newHash = _passwordHasher.Hash(request.NewPassword);
        admin.UpdatePasswordHash(newHash);

        await _repository.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
