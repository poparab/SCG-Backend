namespace SCG.API.Queries.GetAdminDashboard;

public sealed record AdminDashboardDto(
    int TotalAgencies,
    int PendingAgencies,
    int ApprovedAgencies,
    int TotalInquiries,
    int PendingInquiries,
    int ApprovedInquiries,
    int RejectedInquiries,
    decimal TotalWalletBalance,
    IReadOnlyList<RecentAgencyDto> RecentAgencies,
    IReadOnlyList<RecentInquiryDto> RecentInquiries);

public sealed record RecentAgencyDto(
    Guid Id,
    string NameEn,
    string Status,
    DateTime CreatedAt);

public sealed record RecentInquiryDto(
    Guid Id,
    string ReferenceNumber,
    string NationalityCode,
    string Status,
    DateTime CreatedAt);
