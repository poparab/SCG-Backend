using SCG.Application.Abstractions.Messaging;

namespace SCG.InquiryManagement.Application.Queries.GetInquiryById;

public sealed record GetInquiryByIdQuery(Guid InquiryId) : IQuery<InquiryDetailDto>;

public sealed record InquiryDetailDto(
    Guid Id,
    string ReferenceNumber,
    string Status,
    decimal Fee,
    string? PaymentReference,
    string? ResultCode,
    string? RejectionReason,
    DateTime? ProcessedAt,
    string? DocumentUrl,
    Guid? BatchId,
    Guid? AgencyId,
    DateTime CreatedAt,
    InquiryTravelerDto Traveler);

public sealed record InquiryTravelerDto(
    string FirstNameEn,
    string LastNameEn,
    string? FirstNameAr,
    string? LastNameAr,
    string PassportNumber,
    string NationalityCode,
    DateTime DateOfBirth,
    string Gender,
    DateTime TravelDate,
    string? ArrivalAirport,
    string? TransitCountries,
    string? DepartureCountry,
    DateTime? PassportExpiry,
    string? PurposeOfTravel,
    string? FlightNumber);
