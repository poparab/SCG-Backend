using SCG.AgencyManagement.Domain.Enums;

namespace SCG.API.Queries.GetAgencyById;

public sealed record AgencyDetailDto(
    Guid Id,
    string NameEn,
    string NameAr,
    string Email,
    string Phone,
    string CommercialLicenseNumber,
    DateTime CommercialLicenseExpiry,
    string? Address,
    AgencyStatus Status,
    string? RejectionReason,
    decimal WalletBalance,
    string Currency,
    int TotalBatches,
    int TotalInquiries,
    DateTime CreatedAt);
