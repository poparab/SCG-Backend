using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCG.API.Commands.PropagateNationalityToAgencies;
using SCG.API.Contracts.Nationalities;
using SCG.Rules.Application.Commands.AddNationality;
using SCG.Rules.Application.Commands.ToggleNationalityInquiry;
using SCG.Rules.Application.Commands.UpdateNationalityFee;
using SCG.Rules.Application.Queries.GetMasterNationalityList;
using SCG.Rules.Application.Queries.GetNationalities;
using SCG.Rules.Application.Queries.GetNationalityById;
using SCG.Rules.Application.Queries.GetPricingList;

namespace SCG.API.Controllers;

[ApiController]
[Route("api/nationalities")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class NationalitiesController : ControllerBase
{
    private readonly ISender _sender;

    public NationalitiesController(ISender sender) => _sender = sender;

    [HttpGet("master-list")]
    public async Task<IActionResult> GetMasterNationalityList(CancellationToken ct)
    {
        var query = new GetMasterNationalityListQuery();
        var result = await _sender.Send(query, ct);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet]
    public async Task<IActionResult> GetNationalities(
        [FromQuery] string? searchTerm,
        [FromQuery] bool? requiresInquiry,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new GetNationalitiesQuery(searchTerm, requiresInquiry, page, pageSize);
        var result = await _sender.Send(query, ct);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetNationalityById(Guid id, CancellationToken ct)
    {
        var query = new GetNationalityByIdQuery(id);
        var result = await _sender.Send(query, ct);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> AddNationality(AddNationalityRequest request, CancellationToken ct)
    {
        var command = new AddNationalityCommand(
            request.Code,
            request.NameAr,
            request.NameEn,
            request.RequiresInquiry,
            request.DefaultFee);

        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
            return Conflict(new { error = result.Error });

        // Propagate to all approved agencies
        await _sender.Send(new PropagateNationalityToAgenciesCommand(result.Value), ct);

        return Created($"/api/nationalities/{result.Value}", new { id = result.Value });
    }

    [HttpPut("{id:guid}/fee")]
    public async Task<IActionResult> UpdateNationalityFee(Guid id, UpdateNationalityFeeRequest request, CancellationToken ct)
    {
        var command = new UpdateNationalityFeeCommand(id, request.NewFee, request.EffectiveFrom, request.EffectiveTo);
        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpPut("{id:guid}/toggle-inquiry")]
    public async Task<IActionResult> ToggleNationalityInquiry(Guid id, ToggleNationalityInquiryRequest request, CancellationToken ct)
    {
        var command = new ToggleNationalityInquiryCommand(id, request.RequiresInquiry);
        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok();
    }

    [HttpGet("pricing")]
    public async Task<IActionResult> GetPricingList(
        [FromQuery] string? nationalityCode,
        [FromQuery] bool activeOnly = true,
        CancellationToken ct = default)
    {
        var query = new GetPricingListQuery(nationalityCode, activeOnly);
        var result = await _sender.Send(query, ct);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}
