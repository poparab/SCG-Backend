using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SCG.AgencyManagement.Application.Commands.RegisterAgency;
using SCG.API.Contracts.Auth;
using SCG.Identity.Application.Commands.Login;
using SCG.Identity.Application.Commands.RefreshToken;
using SCG.Identity.Application.Commands.RevokeToken;

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
        Response.Cookies.Append("scg_auth", result.Value!.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15),
            Path = "/"
        });

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
        Response.Cookies.Append("scg_auth", result.Value!.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15),
            Path = "/"
        });

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
        Response.Cookies.Delete("scg_auth", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });
        return NoContent();
    }
}
