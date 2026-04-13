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
    private static readonly string[] AllowedDocumentExtensions = [".pdf", ".jpg", ".jpeg", ".png"];
    private const long MaxDocumentSizeBytes = 5 * 1024 * 1024;

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
    public async Task<IActionResult> AddTraveler(Guid id, [FromForm] AddTravelerRequest request, CancellationToken ct)
    {
        if (request.PassportImageDocument is null)
            return BadRequest(new { error = "Passport image document is required." });

        if (request.TicketImageDocument is null)
            return BadRequest(new { error = "Ticket image document is required." });

        var passportImageDocumentPath = await SaveTravelerDocumentAsync(request.PassportImageDocument, "Passport image document", ct);
        if (passportImageDocumentPath is null)
            return BadRequest(new { error = HttpContext.Items[nameof(SaveTravelerDocumentAsync)] as string ?? "Passport image document is invalid." });

        var ticketImageDocumentPath = await SaveTravelerDocumentAsync(request.TicketImageDocument, "Ticket image document", ct);
        if (ticketImageDocumentPath is null)
            return BadRequest(new { error = HttpContext.Items[nameof(SaveTravelerDocumentAsync)] as string ?? "Ticket image document is invalid." });

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
            request.FlightNumber,
            passportImageDocumentPath,
            ticketImageDocumentPath);

        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { id = result.Value });
    }

    [HttpPut("{id:guid}/travelers/{travelerId:guid}")]
    public async Task<IActionResult> UpdateTraveler(Guid id, Guid travelerId, [FromForm] UpdateTravelerRequest request, CancellationToken ct)
    {
        var passportImageDocumentPath = await SaveTravelerDocumentAsync(request.PassportImageDocument, "Passport image document", ct);
        if (request.PassportImageDocument is not null && passportImageDocumentPath is null)
            return BadRequest(new { error = HttpContext.Items[nameof(SaveTravelerDocumentAsync)] as string ?? "Passport image document is invalid." });

        var ticketImageDocumentPath = await SaveTravelerDocumentAsync(request.TicketImageDocument, "Ticket image document", ct);
        if (request.TicketImageDocument is not null && ticketImageDocumentPath is null)
            return BadRequest(new { error = HttpContext.Items[nameof(SaveTravelerDocumentAsync)] as string ?? "Ticket image document is invalid." });

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
            request.FlightNumber,
            passportImageDocumentPath,
            ticketImageDocumentPath);

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

    private async Task<string?> SaveTravelerDocumentAsync(IFormFile? file, string label, CancellationToken ct)
    {
        if (file is null)
            return null;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedDocumentExtensions.Contains(extension))
        {
            HttpContext.Items[nameof(SaveTravelerDocumentAsync)] = $"{label} must be PDF, JPG, JPEG, or PNG.";
            return null;
        }

        if (file.Length > MaxDocumentSizeBytes)
        {
            HttpContext.Items[nameof(SaveTravelerDocumentAsync)] = $"{label} must not exceed 5 MB.";
            return null;
        }

        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "batch-traveler-documents");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream, ct);

        return Path.Combine("uploads", "batch-traveler-documents", fileName).Replace('\\', '/');
    }
}
