using SCG.Application.Abstractions.Messaging;

namespace SCG.Rules.Application.Commands.UpdateNationalityFee;

public sealed record UpdateNationalityFeeCommand(
    Guid NationalityId,
    decimal NewFee,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo) : ICommand;
