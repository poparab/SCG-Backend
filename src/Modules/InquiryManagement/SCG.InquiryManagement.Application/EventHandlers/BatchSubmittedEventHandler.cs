using MediatR;
using SCG.InquiryManagement.Application.Abstractions;
using SCG.InquiryManagement.Domain.Events;
using SCG.Notification.Application.Services;

namespace SCG.InquiryManagement.Application.EventHandlers;

public sealed class BatchSubmittedEventHandler : INotificationHandler<BatchSubmittedDomainEvent>
{
    private readonly IAgencyEmailResolver _agencyEmailResolver;
    private readonly IEmailService _emailService;

    public BatchSubmittedEventHandler(
        IAgencyEmailResolver agencyEmailResolver,
        IEmailService emailService)
    {
        _agencyEmailResolver = agencyEmailResolver;
        _emailService = emailService;
    }

    public async Task Handle(BatchSubmittedDomainEvent notification, CancellationToken cancellationToken)
    {
        var email = await _agencyEmailResolver.GetAgencyEmailAsync(notification.AgencyId, cancellationToken);
        if (string.IsNullOrWhiteSpace(email))
            return;

        await _emailService.SendAsync(
            email,
            "Batch Submission Confirmed",
            $"<p>Your batch <strong>{notification.BatchId}</strong> with {notification.TravelerCount} traveler(s) has been submitted successfully.</p>",
            cancellationToken);
    }
}

