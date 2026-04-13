using SCG.Application.Abstractions.Messaging;

namespace SCG.InquiryManagement.Application.Commands.RejectInquiry;

public sealed record RejectInquiryCommand(Guid InquiryId, string Reason) : ICommand;
