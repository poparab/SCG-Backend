using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SCG.AgencyManagement.Application.Commands.RegisterAgency;
using SCG.API.Contracts.Auth;
using SCG.Identity.Application.Commands.ChangePassword;
using SCG.Identity.Application.Commands.Login;
using SCG.Identity.Application.Commands.RefreshToken;
using SCG.Identity.Application.Commands.RevokeToken;
using System.Security.Claims;

namespace SCG.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender) => _sender = sender;

    private CookieOptions AuthCookieOptions(DateTimeOffset? expires = null) => new()
    {
        HttpOnly = true,
        Secure = Request.IsHttps,
        SameSite = Request.IsHttps ? SameSiteMode.Strict : SameSiteMode.Lax,
        Expires = expires,
        Path = "/"
    };

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterAgencyRequest request, CancellationToken ct)
    {
        var command = new RegisterAgencyCommand(
            request.AgencyName,
            request.CommercialRegNumber,
            request.ContactPersonName,
            request.Email,
            request.Password,
            request.CountryCode,
            request.MobileNumber);

        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Created($"/api/agencies/{result.Value}", new { id = result.Value });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var command = new LoginCommand(request.Email, request.Password, request.LoginType);

        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
            return Unauthorized(new { error = result.Error });

        // Set HttpOnly cookie with JWT (access token)
        Response.Cookies.Append("scg_auth", result.Value!.Token,
            AuthCookieOptions(DateTimeOffset.UtcNow.AddMinutes(15)));

        return Ok(result.Value);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken ct)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
            return Unauthorized(new { error = result.Error });

        // Set new access token cookie
        Response.Cookies.Append("scg_auth", result.Value!.Token,
            AuthCookieOptions(DateTimeOffset.UtcNow.AddMinutes(15)));

        return Ok(result.Value);
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(RevokeTokenRequest request, CancellationToken ct)
    {
        var command = new RevokeTokenCommand(request.RefreshToken);
        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return NoContent();
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("scg_auth", AuthCookieOptions());
        return NoContent();
    }

    [HttpPut("change-password")]
    [Authorize]
    [EnableRateLimiting("api")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken ct)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        var loginType = role is "Admin" or "SuperAdmin" ? "Admin" : "Agency";

        var command = new ChangePasswordCommand(email, request.CurrentPassword, request.NewPassword, loginType);
        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return NoContent();
    }
}
