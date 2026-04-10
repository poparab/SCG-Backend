using MediatR;
using Microsoft.AspNetCore.Mvc;
using SCG.AgencyManagement.Application.Commands.RegisterAgency;
using SCG.API.Contracts.Auth;
using SCG.Identity.Application.Commands.Login;

namespace SCG.API.Controllers;

[ApiController]
[Route("api/auth")]
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

        return Ok(result.Value);
    }
}
