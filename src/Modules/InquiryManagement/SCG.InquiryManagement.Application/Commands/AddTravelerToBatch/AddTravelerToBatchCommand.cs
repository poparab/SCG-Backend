using SCG.Application.Abstractions.Messaging;
using SCG.InquiryManagement.Domain.Enums;

namespace SCG.InquiryManagement.Application.Commands.AddTravelerToBatch;

public sealed record AddTravelerToBatchCommand(
    Guid BatchId,
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
    string? FlightNumber,
    string PassportImageDocumentPath,
    string TicketImageDocumentPath) : ICommand<Guid>;
