using SCG.Application.Abstractions.Messaging;

namespace SCG.InquiryManagement.Application.Commands.ApproveInquiry;

public sealed record ApproveInquiryCommand(Guid InquiryId) : ICommand;
