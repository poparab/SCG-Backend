using SCG.InquiryManagement.Domain.Enums;

namespace SCG.API.Contracts.Batches;

public sealed record CreateBatchRequest(
    Guid AgencyId,
    string Name,
    string? InquiryTypeId,
    string? Notes);

public sealed record AddTravelerRequest(
    string FirstNameEn,
    string LastNameEn,
    string? FirstNameAr,
    string? LastNameAr,
    string PassportNumber,
    string NationalityCode,
    DateTime DateOfBirth,
    TravelerGender Gender,
    DateTime TravelDate,
    string? ArrivalAirport,
    string? TransitCountries,
    DateTime PassportExpiry,
    string DepartureCountry,
    string PurposeOfTravel,
    string? FlightNumber);

public sealed record UpdateTravelerRequest(
    string FirstNameEn,
    string LastNameEn,
    string? FirstNameAr,
    string? LastNameAr,
    string PassportNumber,
    string NationalityCode,
    DateTime DateOfBirth,
    TravelerGender Gender,
    DateTime TravelDate,
    string? ArrivalAirport,
    string? TransitCountries,
    DateTime PassportExpiry,
    string DepartureCountry,
    string PurposeOfTravel,
    string? FlightNumber);

public sealed record SubmitBatchRequest(Guid SubmittedByUserId);
