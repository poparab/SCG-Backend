namespace SCG.Rules.Application.Queries.GetNationalityById;

public sealed record NationalityDetailDto(
    Guid Id,
    string Code,
    string NameAr,
    string NameEn,
    bool RequiresInquiry,
    List<PricingDto> PricingHistory,
    DateTime CreatedAt);

public sealed record PricingDto(
    Guid Id,
    decimal Fee,
    string Currency,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    bool IsActive);
