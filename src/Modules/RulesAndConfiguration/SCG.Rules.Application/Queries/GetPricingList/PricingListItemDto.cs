namespace SCG.Rules.Application.Queries.GetPricingList;

public sealed record PricingListItemDto(
    Guid PricingId,
    string NationalityCode,
    string NationalityNameEn,
    string NationalityNameAr,
    decimal Fee,
    string Currency,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    bool IsActive);
