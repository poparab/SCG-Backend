using SCG.Application.Abstractions.Messaging;

namespace SCG.Rules.Application.Commands.AddNationality;

public sealed record AddNationalityCommand(
    string Code,
    string NameAr,
    string NameEn,
    bool RequiresInquiry,
    decimal DefaultFee) : ICommand<Guid>;
