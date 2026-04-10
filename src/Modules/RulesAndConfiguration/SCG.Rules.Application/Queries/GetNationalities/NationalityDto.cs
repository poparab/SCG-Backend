namespace SCG.Rules.Application.Queries.GetNationalities;

public sealed record NationalityDto(
    Guid Id,
    string Code,
    string NameAr,
    string NameEn,
    bool RequiresInquiry,
    decimal? CurrentFee,
    DateTime CreatedAt);
