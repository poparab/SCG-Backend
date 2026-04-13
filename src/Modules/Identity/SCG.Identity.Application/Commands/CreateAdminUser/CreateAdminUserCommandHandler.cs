using SCG.Application.Abstractions.Messaging;
using SCG.Identity.Application.Abstractions;
using SCG.Identity.Application.Services;
using SCG.Identity.Domain.Entities;
using SCG.SharedKernel;

namespace SCG.Identity.Application.Commands.CreateAdminUser;

public sealed class CreateAdminUserCommandHandler : ICommandHandler<CreateAdminUserCommand, Guid>
{
    private readonly IAdminUserRepository _repository;
    private readonly IPasswordHasher _passwordHasher;

    public CreateAdminUserCommandHandler(
        IAdminUserRepository repository,
        IPasswordHasher passwordHasher)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<Guid>> Handle(CreateAdminUserCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.ExistsByEmailAsync(request.Email, cancellationToken))
            return Result<Guid>.Failure("An admin user with this email already exists.");

        var hash = _passwordHasher.Hash(request.Password);
        var admin = AdminUser.Create(request.FullName, request.Email, hash, request.Role);

        await _repository.AddAsync(admin, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(admin.Id);
    }
}
