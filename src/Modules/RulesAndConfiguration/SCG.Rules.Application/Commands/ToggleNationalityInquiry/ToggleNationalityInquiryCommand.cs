using SCG.Application.Abstractions.Messaging;

namespace SCG.Rules.Application.Commands.ToggleNationalityInquiry;

public sealed record ToggleNationalityInquiryCommand(Guid NationalityId, bool RequiresInquiry) : ICommand;
