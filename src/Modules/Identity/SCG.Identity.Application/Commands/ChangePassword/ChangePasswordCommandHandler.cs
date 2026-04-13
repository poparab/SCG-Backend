using SCG.Application.Abstractions.Messaging;
using SCG.Identity.Application.Services;
using SCG.SharedKernel;

namespace SCG.Identity.Application.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand>
{
    private readonly IUserAuthenticationService _authService;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(
        IUserAuthenticationService authService,
        IPasswordHasher passwordHasher)
    {
        _authService = authService;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _authService.GetUserByEmailAsync(request.Email, request.LoginType, cancellationToken);
        if (user is null)
            return Result.Failure("User not found.");

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            return Result.Failure("Current password is incorrect.");

        var newHash = _passwordHasher.Hash(request.NewPassword);
        await _authService.UpdatePasswordHashAsync(user.UserId, newHash, request.LoginType, cancellationToken);

        return Result.Success();
    }
}
