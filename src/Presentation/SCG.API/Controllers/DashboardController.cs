using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCG.API.Queries.GetAdminDashboard;
using SCG.API.Queries.GetAgencyDashboard;

namespace SCG.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ISender _sender;

    public DashboardController(ISender sender) => _sender = sender;

    [HttpGet("agency/{agencyId:guid}")]
    public async Task<IActionResult> GetAgencyDashboard(Guid agencyId, CancellationToken ct)
    {
        var query = new GetAgencyDashboardQuery(agencyId);
        var result = await _sender.Send(query, ct);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetAdminDashboard(CancellationToken ct)
    {
        var query = new GetAdminDashboardQuery();
        var result = await _sender.Send(query, ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }
}
