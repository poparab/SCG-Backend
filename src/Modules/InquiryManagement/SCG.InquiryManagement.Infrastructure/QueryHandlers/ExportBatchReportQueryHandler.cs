using System.Text;
using Microsoft.EntityFrameworkCore;
using SCG.Application.Abstractions.Messaging;
using SCG.InquiryManagement.Application.Queries.ExportBatchReport;
using SCG.InquiryManagement.Infrastructure.Persistence;
using SCG.SharedKernel;

namespace SCG.InquiryManagement.Infrastructure.QueryHandlers;

internal sealed class ExportBatchReportQueryHandler : IQueryHandler<ExportBatchReportQuery, BatchReportDto>
{
    private readonly InquiryDbContext _db;

    public ExportBatchReportQueryHandler(InquiryDbContext db) => _db = db;

    public async Task<Result<BatchReportDto>> Handle(ExportBatchReportQuery request, CancellationToken cancellationToken)
    {
        var batch = await _db.Batches
            .Include(b => b.Travelers)
                .ThenInclude(t => t.Inquiry)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.BatchId, cancellationToken);

        if (batch is null)
            return Result<BatchReportDto>.Failure("Batch not found.");

        var sb = new StringBuilder();

        sb.AppendLine("Row,First Name (EN),Last Name (EN),First Name (AR),Last Name (AR),Passport Number,Nationality,Date of Birth,Gender,Travel Date,Arrival Airport,Inquiry Reference,Inquiry Status,Fee");

        foreach (var t in batch.Travelers.OrderBy(t => t.RowIndex))
        {
            sb.AppendLine(string.Join(",",
                EscapeCsv(t.RowIndex.ToString()),
                EscapeCsv(t.FirstNameEn),
                EscapeCsv(t.LastNameEn),
                EscapeCsv(t.FirstNameAr ?? ""),
                EscapeCsv(t.LastNameAr ?? ""),
                EscapeCsv(t.PassportNumber),
                EscapeCsv(t.NationalityCode),
                EscapeCsv(t.DateOfBirth.ToString("yyyy-MM-dd")),
                EscapeCsv(t.Gender.ToString()),
                EscapeCsv(t.TravelDate.ToString("yyyy-MM-dd")),
                EscapeCsv(t.ArrivalAirport ?? ""),
                EscapeCsv(t.Inquiry?.ReferenceNumber ?? ""),
                EscapeCsv(t.Inquiry?.Status.ToString() ?? ""),
                EscapeCsv(t.Inquiry?.Fee.ToString("F2") ?? "")));
        }

        var content = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        var fileName = $"Batch_{batch.Name}_{DateTime.UtcNow:yyyyMMdd}.csv";

        return Result<BatchReportDto>.Success(
            new BatchReportDto(fileName, content, "text/csv"));
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
