using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SCG.API.Contracts.Batches;
using SCG.InquiryManagement.Application.Commands.AddTravelerToBatch;
using SCG.InquiryManagement.Application.Commands.CreateBatch;
using SCG.InquiryManagement.Application.Commands.RemoveTravelerFromBatch;
using SCG.InquiryManagement.Application.Commands.SubmitBatch;
using SCG.InquiryManagement.Application.Commands.UpdateTravelerInBatch;
using SCG.InquiryManagement.Application.Queries.ExportBatchReport;
using SCG.InquiryManagement.Application.Queries.GetBatchById;
using SCG.InquiryManagement.Application.Queries.GetBatches;
using SCG.InquiryManagement.Domain.Enums;

namespace SCG.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/batches")]
[Route("api/batches")]
[Authorize]
public class BatchesController : ControllerBase
{
    private readonly ISender _sender;

    public BatchesController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetBatches(
        [FromQuery] Guid agencyId,
        [FromQuery] string? search,
        [FromQuery] BatchStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new GetBatchesQuery(agencyId, search, status, page, pageSize);
        var result = await _sender.Send(query, ct);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBatchById(Guid id, CancellationToken ct)
    {
        var query = new GetBatchByIdQuery(id);
        var result = await _sender.Send(query, ct);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBatch(CreateBatchRequest request, CancellationToken ct)
    {
        var inquiryTypeId = Guid.TryParse(request.InquiryTypeId, out var parsed) ? parsed : new Guid("00000000-0000-0000-0000-000000000001");
        var command = new CreateBatchCommand(
            request.AgencyId,
            GetUserId(),
            request.Name,
            inquiryTypeId,
            request.Notes);

        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetBatchById), new { id = result.Value }, new { id = result.Value });
    }

    [HttpPost("{id:guid}/travelers")]
    public async Task<IActionResult> AddTraveler(Guid id, AddTravelerRequest request, CancellationToken ct)
    {
        var command = new AddTravelerToBatchCommand(
            id,
            request.FirstNameEn,
            request.LastNameEn,
            request.FirstNameAr,
            request.LastNameAr,
            request.PassportNumber,
            request.NationalityCode,
            request.DateOfBirth,
            request.Gender,
            request.TravelDate,
            request.ArrivalAirport,
            request.TransitCountries,
            request.PassportExpiry,
            request.DepartureCountry,
            request.PurposeOfTravel,
            request.FlightNumber);

        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { id = result.Value });
    }

    [HttpPut("{id:guid}/travelers/{travelerId:guid}")]
    public async Task<IActionResult> UpdateTraveler(Guid id, Guid travelerId, UpdateTravelerRequest request, CancellationToken ct)
    {
        var command = new UpdateTravelerInBatchCommand(
            id,
            travelerId,
            request.FirstNameEn,
            request.LastNameEn,
            request.FirstNameAr,
            request.LastNameAr,
            request.PassportNumber,
            request.NationalityCode,
            request.DateOfBirth,
            request.Gender,
            request.TravelDate,
            request.ArrivalAirport,
            request.TransitCountries,
            request.PassportExpiry,
            request.DepartureCountry,
            request.PurposeOfTravel,
            request.FlightNumber);

        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return NoContent();
    }

    [HttpDelete("{id:guid}/travelers/{travelerId:guid}")]
    public async Task<IActionResult> RemoveTraveler(Guid id, Guid travelerId, CancellationToken ct)
    {
        var command = new RemoveTravelerFromBatchCommand(id, travelerId);
        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return NoContent();
    }

    [HttpPost("{id:guid}/submit")]
    [EnableRateLimiting("batch")]
    public async Task<IActionResult> SubmitBatch(Guid id, CancellationToken ct)
    {
        var command = new SubmitBatchCommand(id, GetUserId());
        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/export")]
    public async Task<IActionResult> ExportBatchReport(Guid id, CancellationToken ct)
    {
        var query = new ExportBatchReportQuery(id);
        var result = await _sender.Send(query, ct);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return File(result.Value!.Content, result.Value.ContentType, result.Value.FileName);
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }
}
