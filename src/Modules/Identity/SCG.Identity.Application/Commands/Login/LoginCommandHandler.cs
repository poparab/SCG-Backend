using SCG.Application.Abstractions.Messaging;
using SCG.Identity.Application.Services;
using SCG.SharedKernel;

namespace SCG.Identity.Application.Commands.Login;

internal sealed class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly IUserAuthenticationService _authService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IUserAuthenticationService authService,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _authService = authService;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _authService.GetUserByEmailAsync(request.Email, request.LoginType, cancellationToken);

        if (user is null)
            return Result<LoginResponse>.Failure("Invalid credentials.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result<LoginResponse>.Failure("Invalid credentials.");

        if (request.LoginType.Equals("agency", StringComparison.OrdinalIgnoreCase)
            && user.AgencyStatus is not null
            && !user.AgencyStatus.Equals("Approved", StringComparison.OrdinalIgnoreCase))
        {
            return Result<LoginResponse>.Failure("Account pending approval.");
        }

        var token = _jwtTokenGenerator.GenerateToken(
            user.UserId,
            user.Email,
            user.Role,
            user.AgencyId?.ToString(),
            user.FullName,
            user.AgencyName);

        var response = new LoginResponse(token, user.Email, user.Role, user.AgencyId);
        return Result<LoginResponse>.Success(response);
    }
}
