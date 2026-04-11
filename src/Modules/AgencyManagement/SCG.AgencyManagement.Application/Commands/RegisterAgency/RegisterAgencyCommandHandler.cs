using SCG.AgencyManagement.Application.Abstractions;
using SCG.AgencyManagement.Domain.Entities;
using SCG.Application.Abstractions.Messaging;
using SCG.Identity.Application.Services;
using SCG.SharedKernel;

namespace SCG.AgencyManagement.Application.Commands.RegisterAgency;

public sealed class RegisterAgencyCommandHandler : ICommandHandler<RegisterAgencyCommand, Guid>
{
    private readonly IAgencyRepository _repository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterAgencyCommandHandler(IAgencyRepository repository, IPasswordHasher passwordHasher)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<Guid>> Handle(RegisterAgencyCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await _repository.ExistsByEmailAsync(request.Email, cancellationToken);
        if (emailExists)
            return Result<Guid>.Failure("An agency with this email already exists.");

        if (!string.IsNullOrWhiteSpace(request.CommercialRegNumber))
        {
            var regExists = await _repository.ExistsByCommercialRegAsync(request.CommercialRegNumber, cancellationToken);
            if (regExists)
                return Result<Guid>.Failure("An agency with this commercial registration number already exists.");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);

        var agency = Agency.Register(
            request.AgencyName,
            request.CommercialRegNumber,
            request.ContactPersonName,
            request.Email,
            passwordHash,
            request.CountryCode,
            request.MobileNumber);

        await _repository.AddAsync(agency, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(agency.Id);
    }
}
