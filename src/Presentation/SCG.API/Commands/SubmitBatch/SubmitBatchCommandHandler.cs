using Microsoft.EntityFrameworkCore;
using SCG.AgencyManagement.Infrastructure.Persistence;
using SCG.Application.Abstractions.Messaging;
using SCG.InquiryManagement.Application.Commands.SubmitBatch;
using SCG.InquiryManagement.Domain.Entities;
using SCG.InquiryManagement.Domain.Enums;
using SCG.InquiryManagement.Infrastructure.Persistence;
using SCG.Rules.Infrastructure.Persistence;
using SCG.SharedKernel;

namespace SCG.API.Commands.SubmitBatch;

internal sealed class SubmitBatchCommandHandler : ICommandHandler<SubmitBatchCommand, SubmitBatchResponse>
{
    private readonly InquiryDbContext _inquiryDb;
    private readonly AgencyDbContext _agencyDb;
    private readonly RulesDbContext _rulesDb;

    public SubmitBatchCommandHandler(
        InquiryDbContext inquiryDb,
        AgencyDbContext agencyDb,
        RulesDbContext rulesDb)
    {
        _inquiryDb = inquiryDb;
        _agencyDb = agencyDb;
        _rulesDb = rulesDb;
    }

    public async Task<Result<SubmitBatchResponse>> Handle(SubmitBatchCommand request, CancellationToken cancellationToken)
    {
        // 1. Load batch with travelers
        var batch = await _inquiryDb.Batches
            .Include(b => b.Travelers)
            .FirstOrDefaultAsync(b => b.Id == request.BatchId, cancellationToken);

        if (batch is null)
            return Result<SubmitBatchResponse>.Failure("Batch not found.");

        if (batch.Status != BatchStatus.Draft)
            return Result<SubmitBatchResponse>.Failure("Only batches in Draft status can be submitted.");

        if (batch.Travelers.Count == 0)
            return Result<SubmitBatchResponse>.Failure("Batch must have at least one traveler.");

        // 2. Look up pricing per nationality — group travelers and find most specific match for each
        var now = DateTime.UtcNow;
        var nationalityGroups = batch.Travelers
            .GroupBy(t => t.NationalityCode)
            .ToList();

        var feePerNationality = new Dictionary<string, decimal>();
        decimal totalFee = 0m;

        foreach (var group in nationalityGroups)
        {
            var nationalityCode = group.Key;
            decimal fee;

            // Priority 1: Agency-specific custom fee from AgencyNationalities
            var agencyNationality = await _rulesDb.AgencyNationalities
                .Include(an => an.Nationality)
                .Where(an => an.AgencyId == batch.AgencyId
                             && an.Nationality.Code == nationalityCode
                             && an.IsEnabled
                             && an.CustomFee != null)
                .FirstOrDefaultAsync(cancellationToken);

            if (agencyNationality is not null)
            {
                fee = agencyNationality.CustomFee!.Value;
            }
            else
            {
                // Priority 2: Default pricing (no agency, no category override)
                var pricing = await _rulesDb.Pricings
                    .Where(p => p.InquiryTypeId == batch.InquiryTypeId
                                && p.IsActive
                                && p.EffectiveFrom <= now
                                && (p.EffectiveTo == null || p.EffectiveTo > now)
                                && p.AgencyId == null
                                && p.AgencyCategoryId == null
                                && p.NationalityCode == nationalityCode)
                    .FirstOrDefaultAsync(cancellationToken);

                if (pricing is null)
                    return Result<SubmitBatchResponse>.Failure(
                        "No active pricing found for this nationality and inquiry type.");

                fee = pricing.Fee;
            }

            feePerNationality[nationalityCode] = fee;
            totalFee += fee * group.Count();
        }

        // 3. Load wallet and debit
        var wallet = await _agencyDb.Wallets
            .FirstOrDefaultAsync(w => w.AgencyId == batch.AgencyId, cancellationToken);

        if (wallet is null)
            return Result<SubmitBatchResponse>.Failure("Agency wallet not found.");

        // 4. Generate batch reference: BTH-YYYYMMDD-NNNN
        var today = DateTime.UtcNow.Date;
        var todayBatchCount = await _inquiryDb.Batches
            .CountAsync(b => b.SubmittedAt != null && b.SubmittedAt.Value.Date == today, cancellationToken);
        var batchRef = $"BTH-{today:yyyyMMdd}-{(todayBatchCount + 1):D4}";

        var debitResult = wallet.Debit(totalFee, batchRef, $"Batch submission: {batch.Name}", request.SubmittedByUserId.ToString());
        if (debitResult.IsFailure)
            return Result<SubmitBatchResponse>.Failure(debitResult.Error!);

        // 5. Create individual inquiries for each traveler
        var inquiryCounter = await _inquiryDb.Inquiries
            .CountAsync(i => i.CreatedAt.Date == today, cancellationToken);

        foreach (var traveler in batch.Travelers)
        {
            inquiryCounter++;
            var inquiryRef = $"INQ-{today:yyyyMMdd}-{inquiryCounter:D6}";

            var inquiry = Inquiry.Create(
                batch.InquiryTypeId,
                inquiryRef,
                traveler.FirstNameEn,
                traveler.LastNameEn,
                traveler.PassportNumber,
                traveler.NationalityCode,
                traveler.DateOfBirth,
                traveler.Gender,
                traveler.TravelDate,
                feePerNationality[traveler.NationalityCode],
                batch.AgencyId,
                batch.Id);

            await _inquiryDb.Inquiries.AddAsync(inquiry, cancellationToken);
            traveler.LinkInquiry(inquiry.Id);
        }

        // 6. Mark batch as submitted with payment info
        batch.MarkPaymentComplete(batchRef, totalFee);

        // 7. Save all changes atomically
        await _agencyDb.SaveChangesAsync(cancellationToken);
        await _inquiryDb.SaveChangesAsync(cancellationToken);

        return Result<SubmitBatchResponse>.Success(new SubmitBatchResponse(
            batchRef,
            batch.Travelers.Count,
            totalFee,
            wallet.Balance));
    }
}
