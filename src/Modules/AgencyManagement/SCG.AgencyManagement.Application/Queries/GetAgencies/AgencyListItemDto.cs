using SCG.AgencyManagement.Domain.Enums;

namespace SCG.AgencyManagement.Application.Queries.GetAgencies;

public sealed record AgencyListItemDto(
    Guid Id,
    string NameEn,
    string NameAr,
    string Email,
    string CommercialLicenseNumber,
    AgencyStatus Status,
    decimal WalletBalance,
    DateTime CreatedAt);
