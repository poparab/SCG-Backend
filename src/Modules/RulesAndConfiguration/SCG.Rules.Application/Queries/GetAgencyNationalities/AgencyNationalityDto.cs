namespace SCG.Rules.Application.Queries.GetAgencyNationalities;

public sealed record AgencyNationalityDto(
    Guid Id,
    Guid AgencyId,
    Guid NationalityId,
    string NationalityCode,
    string NationalityNameAr,
    string NationalityNameEn,
    bool RequiresInquiry,
    decimal DefaultFee,
    decimal? CustomFee,
    decimal EffectiveFee,
    bool IsEnabled);
