using MediatR;
using SCG.InquiryManagement.Application.Abstractions;
using SCG.InquiryManagement.Domain.Events;
using SCG.Notification.Application.Services;

namespace SCG.InquiryManagement.Application.EventHandlers;

public sealed class InquiryApprovedEventHandler : INotificationHandler<InquiryApprovedDomainEvent>
{
    private readonly IInquiryRepository _inquiryRepository;
    private readonly IAgencyEmailResolver _agencyEmailResolver;
    private readonly IEmailService _emailService;

    public InquiryApprovedEventHandler(
        IInquiryRepository inquiryRepository,
        IAgencyEmailResolver agencyEmailResolver,
        IEmailService emailService)
    {
        _inquiryRepository = inquiryRepository;
        _agencyEmailResolver = agencyEmailResolver;
        _emailService = emailService;
    }

    public async Task Handle(InquiryApprovedDomainEvent notification, CancellationToken cancellationToken)
    {
        var inquiry = await _inquiryRepository.GetByIdAsync(notification.InquiryId, cancellationToken);
        if (inquiry is null || inquiry.AgencyId is null)
            return;

        var email = await _agencyEmailResolver.GetAgencyEmailAsync(inquiry.AgencyId.Value, cancellationToken);
        if (string.IsNullOrWhiteSpace(email))
            return;

        await _emailService.SendAsync(
            email,
            "Inquiry Approved",
            $"<p>Inquiry <strong>{notification.ReferenceNumber}</strong> has been approved.</p>",
            cancellationToken);
    }
}
