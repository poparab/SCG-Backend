using System.Transactions;
using SCG.Application.Abstractions.Messaging;
using SCG.Application.Abstractions.Services;
using SCG.InquiryManagement.Application.Abstractions;
using SCG.InquiryManagement.Domain.Entities;
using SCG.InquiryManagement.Domain.Enums;
using SCG.SharedKernel;

namespace SCG.InquiryManagement.Application.Commands.SubmitBatch;

public sealed class SubmitBatchCommandHandler : ICommandHandler<SubmitBatchCommand, SubmitBatchResponse>
{
    private readonly IBatchRepository _batchRepo;
    private readonly IInquiryRepository _inquiryRepo;
    private readonly IWalletService _walletService;
    private readonly IPricingService _pricingService;

    public SubmitBatchCommandHandler(
        IBatchRepository batchRepo,
        IInquiryRepository inquiryRepo,
        IWalletService walletService,
        IPricingService pricingService)
    {
        _batchRepo = batchRepo;
        _inquiryRepo = inquiryRepo;
        _walletService = walletService;
        _pricingService = pricingService;
    }

    public async Task<Result<SubmitBatchResponse>> Handle(SubmitBatchCommand request, CancellationToken cancellationToken)
    {
        // 1. Load batch with travelers
        var batch = await _batchRepo.GetByIdWithTravelersAsync(request.BatchId, cancellationToken);

        if (batch is null)
            return Result<SubmitBatchResponse>.Failure("Batch not found.");

        if (batch.Status != BatchStatus.Draft)
            return Result<SubmitBatchResponse>.Failure("Only batches in Draft status can be submitted.");

        if (batch.Travelers.Count == 0)
            return Result<SubmitBatchResponse>.Failure("Batch must have at least one traveler.");

        // 2. Look up pricing per nationality
        var nationalityGroups = batch.Travelers
            .GroupBy(t => t.NationalityCode)
            .ToList();

        var feePerNationality = new Dictionary<string, decimal>();
        decimal totalFee = 0m;

        foreach (var group in nationalityGroups)
        {
            var nationalityCode = group.Key;
            var fee = await _pricingService.GetFeeAsync(
                nationalityCode, batch.InquiryTypeId, batch.AgencyId, cancellationToken);

            if (fee is null)
                return Result<SubmitBatchResponse>.Failure(
                    $"No active pricing found for nationality '{nationalityCode}' and this inquiry type.");

            feePerNationality[nationalityCode] = fee.Value;
            totalFee += fee.Value * group.Count();
        }

        // 3. Generate unique references that are safe under parallel submissions.
        var now = DateTime.UtcNow;
        var today = now.Date;
        var batchRef = $"BTH-{now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..27];

        // 4. Create individual inquiries for each traveler

        foreach (var traveler in batch.Travelers)
        {
            var inquiryRef = $"INQ-{now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..27];

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
                batch.Id,
                traveler.FirstNameAr,
                traveler.LastNameAr,
                traveler.ArrivalAirport,
                traveler.TransitCountries,
                traveler.DepartureCountry,
                traveler.PassportExpiry,
                traveler.PurposeOfTravel,
                traveler.FlightNumber);

            await _inquiryRepo.AddAsync(inquiry, cancellationToken);
            traveler.LinkInquiry(inquiry.Id);
        }

        // 5. Mark batch as submitted with payment info
        batch.MarkPaymentComplete(batchRef, totalFee);

        // 6. Debit wallet and save all changes within a transaction scope
        using var txScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var debitResult = await _walletService.DebitAsync(
            batch.AgencyId, totalFee, batchRef,
            $"Batch submission: {batch.Name}",
            request.SubmittedByUserId.ToString(),
            cancellationToken);

        if (debitResult.IsFailure)
            return Result<SubmitBatchResponse>.Failure(debitResult.Error!);

        await _batchRepo.SaveChangesAsync(cancellationToken);

        txScope.Complete();

        return Result<SubmitBatchResponse>.Success(new SubmitBatchResponse(
            batchRef,
            batch.Travelers.Count,
            totalFee,
            debitResult.Value));
    }
}
