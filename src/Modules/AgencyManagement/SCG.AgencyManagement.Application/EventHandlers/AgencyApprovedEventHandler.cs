using MediatR;
using SCG.AgencyManagement.Domain.Events;

namespace SCG.AgencyManagement.Application.EventHandlers;

public sealed class AgencyApprovedEventHandler : INotificationHandler<AgencyApprovedDomainEvent>
{
    public Task Handle(AgencyApprovedDomainEvent notification, CancellationToken cancellationToken)
    {
        // TODO: Wire to Notification module to send approval email
        // Placeholder: domain event is dispatched and handler is invoked
        return Task.CompletedTask;
    }
}
