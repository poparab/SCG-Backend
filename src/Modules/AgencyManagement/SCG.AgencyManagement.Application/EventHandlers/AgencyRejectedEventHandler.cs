using MediatR;
using SCG.AgencyManagement.Application.Abstractions;
using SCG.AgencyManagement.Domain.Events;
using SCG.Notification.Application.Services;

namespace SCG.AgencyManagement.Application.EventHandlers;

public sealed class AgencyRejectedEventHandler : INotificationHandler<AgencyRejectedDomainEvent>
{
    private readonly IAgencyRepository _agencyRepository;
    private readonly IEmailService _emailService;

    public AgencyRejectedEventHandler(
        IAgencyRepository agencyRepository,
        IEmailService emailService)
    {
        _agencyRepository = agencyRepository;
        _emailService = emailService;
    }

    public async Task Handle(AgencyRejectedDomainEvent notification, CancellationToken cancellationToken)
    {
        var agency = await _agencyRepository.GetByIdAsync(notification.AgencyId, cancellationToken);
        if (agency is null)
            return;

        await _emailService.SendAsync(
            agency.Email,
            "Agency Registration Rejected",
            $"<p>Your agency registration was rejected.</p><p>Reason: {System.Net.WebUtility.HtmlEncode(notification.Reason)}</p>",
            cancellationToken);
    }
}
