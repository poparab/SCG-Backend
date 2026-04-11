using MediatR;
using SCG.InquiryManagement.Domain.Events;

namespace SCG.InquiryManagement.Application.EventHandlers;

public sealed class BatchSubmittedEventHandler : INotificationHandler<BatchSubmittedDomainEvent>
{
    public Task Handle(BatchSubmittedDomainEvent notification, CancellationToken cancellationToken)
    {
        // TODO: Queue batch for clearance engine processing via Hangfire
        // Placeholder: domain event is dispatched and handler is invoked
        return Task.CompletedTask;
    }
}
