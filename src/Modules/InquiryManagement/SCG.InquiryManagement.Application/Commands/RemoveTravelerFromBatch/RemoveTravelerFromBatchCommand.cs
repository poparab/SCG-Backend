using SCG.Application.Abstractions.Messaging;

namespace SCG.InquiryManagement.Application.Commands.RemoveTravelerFromBatch;

public sealed record RemoveTravelerFromBatchCommand(Guid BatchId, Guid TravelerId) : ICommand;
