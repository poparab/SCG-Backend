using SCG.Application.Abstractions.Messaging;

namespace SCG.AgencyManagement.Application.Commands.RegisterAgency;

public sealed record RegisterAgencyCommand(
    string AgencyName,
    string? CommercialRegNumber,
    string ContactPersonName,
    string Email,
    string Password,
    string CountryCode,
    string MobileNumber) : ICommand<Guid>;
