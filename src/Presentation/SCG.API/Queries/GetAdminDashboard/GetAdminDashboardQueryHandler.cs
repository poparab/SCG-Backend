using Microsoft.EntityFrameworkCore;
using SCG.AgencyManagement.Domain.Enums;
using SCG.AgencyManagement.Infrastructure.Persistence;
using SCG.Application.Abstractions.Messaging;
using SCG.InquiryManagement.Domain.Enums;
using SCG.InquiryManagement.Infrastructure.Persistence;
using SCG.SharedKernel;

namespace SCG.API.Queries.GetAdminDashboard;

internal sealed class GetAdminDashboardQueryHandler : IQueryHandler<GetAdminDashboardQuery, AdminDashboardDto>
{
    private readonly AgencyDbContext _agencyDb;
    private readonly InquiryDbContext _inquiryDb;

    public GetAdminDashboardQueryHandler(AgencyDbContext agencyDb, InquiryDbContext inquiryDb)
    {
        _agencyDb = agencyDb;
        _inquiryDb = inquiryDb;
    }

    public async Task<Result<AdminDashboardDto>> Handle(GetAdminDashboardQuery request, CancellationToken cancellationToken)
    {
        // Agency stats
        var totalAgencies = await _agencyDb.Agencies.CountAsync(cancellationToken);
        var pendingAgencies = await _agencyDb.Agencies
            .CountAsync(a => a.Status == AgencyStatus.PendingReview, cancellationToken);
        var approvedAgencies = await _agencyDb.Agencies
            .CountAsync(a => a.Status == AgencyStatus.Approved, cancellationToken);

        // Inquiry stats
        var totalInquiries = await _inquiryDb.Inquiries.CountAsync(cancellationToken);
        var pendingInquiries = await _inquiryDb.Inquiries
            .CountAsync(i => i.Status == InquiryStatus.Submitted || i.Status == InquiryStatus.UnderProcessing, cancellationToken);
        var approvedInquiries = await _inquiryDb.Inquiries
            .CountAsync(i => i.Status == InquiryStatus.Approved, cancellationToken);
        var rejectedInquiries = await _inquiryDb.Inquiries
            .CountAsync(i => i.Status == InquiryStatus.Rejected, cancellationToken);

        // Total wallet balance
        var totalWalletBalance = await _agencyDb.Wallets.SumAsync(w => w.Balance, cancellationToken);

        // Recent agencies
        var recentAgencies = await _agencyDb.Agencies
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
            .Select(a => new RecentAgencyDto(a.Id, a.NameEn, a.Status.ToString(), a.CreatedAt))
            .ToListAsync(cancellationToken);

        // Recent inquiries
        var recentInquiries = await _inquiryDb.Inquiries
            .OrderByDescending(i => i.CreatedAt)
            .Take(10)
            .Select(i => new RecentInquiryDto(i.Id, i.ReferenceNumber, i.NationalityCode, i.Status.ToString(), i.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<AdminDashboardDto>.Success(new AdminDashboardDto(
            totalAgencies,
            pendingAgencies,
            approvedAgencies,
            totalInquiries,
            pendingInquiries,
            approvedInquiries,
            rejectedInquiries,
            totalWalletBalance,
            recentAgencies,
            recentInquiries));
    }
}
