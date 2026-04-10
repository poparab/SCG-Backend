namespace SCG.API.Contracts.Auth;

public sealed record RegisterAgencyRequest(
    string AgencyName,
    string? CommercialRegNumber,
    string ContactPersonName,
    string Email,
    string Password,
    string CountryCode,
    string MobileNumber);
