using MediatR;
using SCG.AgencyManagement.Application.Abstractions;
using SCG.AgencyManagement.Domain.Events;
using SCG.Notification.Application.Services;

namespace SCG.AgencyManagement.Application.EventHandlers;

public sealed class AgencyApprovedEventHandler : INotificationHandler<AgencyApprovedDomainEvent>
{
    private readonly IAgencyRepository _agencyRepository;
    private readonly IEmailService _emailService;

    public AgencyApprovedEventHandler(
        IAgencyRepository agencyRepository,
        IEmailService emailService)
    {
        _agencyRepository = agencyRepository;
        _emailService = emailService;
    }

    public async Task Handle(AgencyApprovedDomainEvent notification, CancellationToken cancellationToken)
    {
        var agency = await _agencyRepository.GetByIdAsync(notification.AgencyId, cancellationToken);
        if (agency is null)
            return;

        await _emailService.SendAsync(
            agency.Email,
            "Agency Registration Approved",
            "<p>Your agency has been approved. You can now log in and submit inquiries.</p>",
            cancellationToken);
    }
}

