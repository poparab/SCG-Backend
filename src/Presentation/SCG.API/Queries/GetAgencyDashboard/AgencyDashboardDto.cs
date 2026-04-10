using SCG.AgencyManagement.Application.Queries.GetWalletByAgencyId;

namespace SCG.API.Queries.GetAgencyDashboard;

public sealed record AgencyDashboardDto(
    string AgencyName,
    string AgencyNameEn,
    string AgencyStatus,
    DateTime RegisteredAt,
    DateTime? LastActivityAt,
    decimal WalletBalance,
    string Currency,
    int TotalBatches,
    int DraftBatches,
    int SubmittedBatches,
    int TotalInquiries,
    int ApprovedInquiries,
    int RejectedInquiries,
    int PendingInquiries,
    IReadOnlyList<RecentBatchDto> RecentBatches,
    IReadOnlyList<WalletTransactionDto> RecentTransactions);

public sealed record RecentBatchDto(
    Guid Id,
    string Name,
    string Status,
    int TravelerCount,
    DateTime CreatedAt);
