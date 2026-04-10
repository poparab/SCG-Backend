using SCG.Application.Abstractions.Messaging;

namespace SCG.InquiryManagement.Application.Commands.SubmitBatch;

public sealed record SubmitBatchCommand(Guid BatchId, Guid SubmittedByUserId) : ICommand<SubmitBatchResponse>;

public sealed record SubmitBatchResponse(
    string BatchReference,
    int TotalTravelers,
    decimal TotalFee,
    decimal RemainingBalance);
