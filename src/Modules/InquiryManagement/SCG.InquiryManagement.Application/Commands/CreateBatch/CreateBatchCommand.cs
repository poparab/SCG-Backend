using SCG.Application.Abstractions.Messaging;

namespace SCG.InquiryManagement.Application.Commands.CreateBatch;

public sealed record CreateBatchCommand(
    Guid AgencyId,
    Guid CreatedByUserId,
    string Name,
    Guid InquiryTypeId,
    string? Notes) : ICommand<Guid>;
