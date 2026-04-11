using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCG.AgencyManagement.Application.Commands.ApproveAgency;
using SCG.AgencyManagement.Application.Commands.CreditWallet;
using SCG.AgencyManagement.Application.Commands.RejectAgency;
using SCG.AgencyManagement.Application.Queries.GetAgencies;
using SCG.AgencyManagement.Application.Queries.GetWalletByAgencyId;
using SCG.AgencyManagement.Application.Queries.GetWalletTransactions;
using SCG.AgencyManagement.Domain.Enums;
using SCG.API.Commands.PropagateNationalitiesToAgency;
using SCG.API.Contracts.Agencies;
using SCG.API.Queries.GetAgencyById;
using SCG.Rules.Application.Commands.UpdateAgencyNationality;
using SCG.Rules.Application.Queries.GetAgencyNationalities;

namespace SCG.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/agencies")]
[Route("api/agencies")]
[Authorize]
public class AgenciesController : ControllerBase
{
    private readonly ISender _sender;

    public AgenciesController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetAgencies(
        [FromQuery] string? searchTerm,
        [FromQuery] AgencyStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new GetAgenciesQuery(searchTerm, status, page, pageSize);
        var result = await _sender.Send(query, ct);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAgencyById(Guid id, CancellationToken ct)
    {
        var query = new GetAgencyByIdQuery(id);
        var result = await _sender.Send(query, ct);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> ApproveAgency(Guid id, CancellationToken ct)
    {
        var command = new ApproveAgencyCommand(id);
        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        // Propagate all existing nationalities to the newly approved agency
        await _sender.Send(new PropagateNationalitiesToAgencyCommand(id), ct);

        return Ok();
    }

    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> RejectAgency(Guid id, RejectAgencyRequest request, CancellationToken ct)
    {
        var command = new RejectAgencyCommand(id, request.Reason);
        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok();
    }

    [HttpPost("{id:guid}/wallet/credit")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> CreditWallet(Guid id, [FromForm] CreditWalletRequest request, CancellationToken ct)
    {
        string? evidenceFileName = null;

        if (request.Evidence is not null)
        {
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(request.Evidence.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { error = "Evidence file must be PDF, JPG, or PNG." });

            if (request.Evidence.Length > 5 * 1024 * 1024)
                return BadRequest(new { error = "Evidence file must not exceed 5 MB." });

            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "wallet-evidence");
            Directory.CreateDirectory(uploadsDir);

            evidenceFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsDir, evidenceFileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await request.Evidence.CopyToAsync(stream, ct);
        }

        var command = new CreditWalletCommand(
            id,
            request.Amount,
            request.PaymentMethod,
            request.Reference ?? string.Empty,
            request.Notes,
            evidenceFileName);

        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/wallet")]
    public async Task<IActionResult> GetWallet(Guid id, CancellationToken ct)
    {
        var query = new GetWalletQuery(id);
        var result = await _sender.Send(query, ct);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/wallet/transactions")]
    public async Task<IActionResult> GetWalletTransactions(
        Guid id,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? transactionType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new GetWalletTransactionsQuery(id, from, to, transactionType, page, pageSize);
        var result = await _sender.Send(query, ct);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/nationalities")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetAgencyNationalities(Guid id, CancellationToken ct)
    {
        var query = new GetAgencyNationalitiesQuery(id);
        var result = await _sender.Send(query, ct);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:guid}/nationalities/{nationalityId:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> UpdateAgencyNationality(
        Guid id, Guid nationalityId, UpdateAgencyNationalityRequest request, CancellationToken ct)
    {
        var command = new UpdateAgencyNationalityCommand(id, nationalityId, request.CustomFee, request.IsEnabled);
        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok();
    }
}
