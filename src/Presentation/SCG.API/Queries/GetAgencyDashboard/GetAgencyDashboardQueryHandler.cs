using Microsoft.EntityFrameworkCore;
using SCG.AgencyManagement.Application.Queries.GetWalletByAgencyId;
using SCG.AgencyManagement.Infrastructure.Persistence;
using SCG.Application.Abstractions.Messaging;
using SCG.InquiryManagement.Domain.Enums;
using SCG.InquiryManagement.Infrastructure.Persistence;
using SCG.SharedKernel;

namespace SCG.API.Queries.GetAgencyDashboard;

internal sealed class GetAgencyDashboardQueryHandler : IQueryHandler<GetAgencyDashboardQuery, AgencyDashboardDto>
{
    private readonly AgencyDbContext _agencyDb;
    private readonly InquiryDbContext _inquiryDb;

    public GetAgencyDashboardQueryHandler(AgencyDbContext agencyDb, InquiryDbContext inquiryDb)
    {
        _agencyDb = agencyDb;
        _inquiryDb = inquiryDb;
    }

    public async Task<Result<AgencyDashboardDto>> Handle(GetAgencyDashboardQuery request, CancellationToken cancellationToken)
    {
        var agency = await _agencyDb.Set<SCG.AgencyManagement.Domain.Entities.Agency>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AgencyId, cancellationToken);

        if (agency is null)
            return Result<AgencyDashboardDto>.Failure("Agency not found.");

        var wallet = await _agencyDb.Wallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.AgencyId == request.AgencyId, cancellationToken);

        if (wallet is null)
            return Result<AgencyDashboardDto>.Failure("Agency wallet not found.");

        // Batch stats
        var batchQuery = _inquiryDb.Batches.Where(b => b.AgencyId == request.AgencyId);
        var totalBatches = await batchQuery.CountAsync(cancellationToken);
        var draftBatches = await batchQuery.CountAsync(b => b.Status == BatchStatus.Draft, cancellationToken);
        var submittedBatches = await batchQuery.CountAsync(
            b => b.Status != BatchStatus.Draft, cancellationToken);

        // Inquiry stats
        var inquiryQuery = _inquiryDb.Inquiries.Where(i => i.AgencyId == request.AgencyId);
        var totalInquiries = await inquiryQuery.CountAsync(cancellationToken);
        var approvedInquiries = await inquiryQuery.CountAsync(i => i.Status == InquiryStatus.Approved, cancellationToken);
        var rejectedInquiries = await inquiryQuery.CountAsync(i => i.Status == InquiryStatus.Rejected, cancellationToken);
        var pendingInquiries = await inquiryQuery.CountAsync(
            i => i.Status == InquiryStatus.Submitted || i.Status == InquiryStatus.UnderProcessing, cancellationToken);

        // Recent batches
        var recentBatches = await batchQuery
            .OrderByDescending(b => b.CreatedAt)
            .Take(5)
            .Select(b => new RecentBatchDto(b.Id, b.Name, b.Status.ToString(), b.TravelerCount, b.CreatedAt))
            .ToListAsync(cancellationToken);

        // Recent transactions
        var recentTransactions = await _agencyDb.WalletTransactions
            .Where(t => t.WalletId == wallet.Id)
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .Select(t => new WalletTransactionDto(
                t.Id, t.Amount, t.Type.ToString(), t.ReferenceNumber, t.Notes, t.CreatedBy, t.CreatedAt))
            .ToListAsync(cancellationToken);

        // Last activity: most recent batch or inquiry creation
        DateTime? lastActivityAt = null;
        var latestBatch = await batchQuery.OrderByDescending(b => b.CreatedAt).Select(b => (DateTime?)b.CreatedAt).FirstOrDefaultAsync(cancellationToken);
        var latestInquiry = await inquiryQuery.OrderByDescending(i => i.CreatedAt).Select(i => (DateTime?)i.CreatedAt).FirstOrDefaultAsync(cancellationToken);
        if (latestBatch.HasValue && latestInquiry.HasValue)
            lastActivityAt = latestBatch > latestInquiry ? latestBatch : latestInquiry;
        else
            lastActivityAt = latestBatch ?? latestInquiry;

        return Result<AgencyDashboardDto>.Success(new AgencyDashboardDto(
            agency.NameAr,
            agency.NameEn,
            agency.Status.ToString(),
            agency.CreatedAt,
            lastActivityAt,
            wallet.Balance,
            wallet.Currency,
            totalBatches,
            draftBatches,
            submittedBatches,
            totalInquiries,
            approvedInquiries,
            rejectedInquiries,
            pendingInquiries,
            recentBatches,
            recentTransactions));
    }
}
