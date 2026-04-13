using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SCG.API.Contracts.AdminUsers;
using SCG.Identity.Application.Commands.CreateAdminUser;
using SCG.Identity.Application.Commands.ResetAdminPassword;
using SCG.Identity.Application.Commands.ToggleAdminActive;
using SCG.Identity.Application.Commands.UpdateAdminUser;
using SCG.Identity.Application.Queries.GetAdminUserById;
using SCG.Identity.Application.Queries.GetAdminUsers;

namespace SCG.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin-users")]
[Route("api/admin-users")]
[Authorize(Roles = "SuperAdmin")]
[EnableRateLimiting("api")]
public class AdminUsersController : ControllerBase
{
    private readonly ISender _sender;

    public AdminUsersController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAdminUsers(
        [FromQuery] string? searchTerm,
        [FromQuery] string? role,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new GetAdminUsersQuery(searchTerm, role, isActive, page, pageSize);
        var result = await _sender.Send(query, ct);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAdminUserById(Guid id, CancellationToken ct)
    {
        var query = new GetAdminUserByIdQuery(id);
        var result = await _sender.Send(query, ct);

        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> CreateAdminUser(CreateAdminUserRequest request, CancellationToken ct)
    {
        var command = new CreateAdminUserCommand(request.FullName, request.Email, request.Password, request.Role);
        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Created($"/api/admin-users/{result.Value}", new { id = result.Value });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAdminUser(Guid id, UpdateAdminUserRequest request, CancellationToken ct)
    {
        var command = new UpdateAdminUserCommand(id, request.FullName, request.Role);
        var result = await _sender.Send(command, ct);

        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:guid}/toggle-active")]
    public async Task<IActionResult> ToggleActive(Guid id, CancellationToken ct)
    {
        var command = new ToggleAdminActiveCommand(id);
        var result = await _sender.Send(command, ct);

        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, ResetAdminPasswordRequest request, CancellationToken ct)
    {
        var command = new ResetAdminPasswordCommand(id, request.NewPassword);
        var result = await _sender.Send(command, ct);

        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }
}
