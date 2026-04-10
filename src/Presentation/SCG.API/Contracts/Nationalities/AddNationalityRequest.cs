namespace SCG.API.Contracts.Nationalities;

public sealed record AddNationalityRequest(
    string Code,
    string NameAr,
    string NameEn,
    bool RequiresInquiry,
    decimal DefaultFee);
