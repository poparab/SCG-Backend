using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SCG.API.Contracts.Inquiries;
using SCG.InquiryManagement.Application.Commands.ApproveInquiry;
using SCG.InquiryManagement.Application.Commands.RejectInquiry;
using SCG.InquiryManagement.Application.Queries.GetInquiries;
using SCG.InquiryManagement.Application.Queries.GetInquiryById;
using SCG.InquiryManagement.Domain.Enums;

namespace SCG.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/inquiries")]
[Route("api/inquiries")]
[Authorize]
public class InquiriesController : ControllerBase
{
    private readonly ISender _sender;

    public InquiriesController(ISender sender) => _sender = sender;

    [HttpGet]
    [EnableRateLimiting("api")]
    public async Task<IActionResult> GetInquiries(
        [FromQuery] Guid? agencyId,
        [FromQuery] string? search,
        [FromQuery] InquiryStatus? status,
        [FromQuery] string? nationality,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new GetInquiriesQuery(agencyId, search, status, nationality, dateFrom, page, pageSize);
        var result = await _sender.Send(query, ct);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("{id:guid}")]
    [EnableRateLimiting("api")]
    public async Task<IActionResult> GetInquiryById(Guid id, CancellationToken ct)
    {
        var query = new GetInquiryByIdQuery(id);
        var result = await _sender.Send(query, ct);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [EnableRateLimiting("api")]
    public async Task<IActionResult> ApproveInquiry(Guid id, CancellationToken ct)
    {
        var command = new ApproveInquiryCommand(id);
        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok();
    }

    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [EnableRateLimiting("api")]
    public async Task<IActionResult> RejectInquiry(Guid id, [FromBody] RejectInquiryRequest request, CancellationToken ct)
    {
        var command = new RejectInquiryCommand(id, request.Reason);
        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok();
    }
}
