using SCG.Application.Abstractions.Messaging;

namespace SCG.InquiryManagement.Application.Queries.GetBatchById;

public sealed record GetBatchByIdQuery(Guid BatchId) : IQuery<BatchDetailDto>;

public sealed record BatchDetailDto(
    Guid Id,
    string Name,
    string? Notes,
    string Status,
    int TravelerCount,
    DateTime CreatedAt,
    DateTime? SubmittedAt,
    decimal? TotalFee,
    string? PaymentReference,
    IReadOnlyList<BatchTravelerDto> Travelers);

public sealed record BatchTravelerDto(
    Guid Id,
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
    DateTime PassportExpiry,
    string DepartureCountry,
    string PurposeOfTravel,
    string? FlightNumber,
    Guid? InquiryId,
    string? InquiryStatus,
    string? InquiryReferenceNumber);
