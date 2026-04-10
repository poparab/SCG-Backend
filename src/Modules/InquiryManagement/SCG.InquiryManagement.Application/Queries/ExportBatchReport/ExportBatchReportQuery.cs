using SCG.Application.Abstractions.Messaging;

namespace SCG.InquiryManagement.Application.Queries.ExportBatchReport;

public sealed record ExportBatchReportQuery(Guid BatchId) : IQuery<BatchReportDto>;

public sealed record BatchReportDto(string FileName, byte[] Content, string ContentType);
