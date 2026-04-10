namespace SCG.API.Contracts.Nationalities;

public sealed record UpdateNationalityFeeRequest(
    decimal NewFee,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo);
