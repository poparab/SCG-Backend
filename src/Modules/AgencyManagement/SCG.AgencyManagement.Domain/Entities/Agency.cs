using SCG.AgencyManagement.Domain.Enums;
using SCG.AgencyManagement.Domain.Events;
using SCG.SharedKernel;

namespace SCG.AgencyManagement.Domain.Entities;

public sealed class Agency : AggregateRoot<Guid>
{
    public string NameAr { get; private set; } = default!;
    public string NameEn { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string Phone { get; private set; } = default!;
    public string? CommercialLicenseNumber { get; private set; }
    public DateTime CommercialLicenseExpiry { get; private set; }
    public string? CommercialLicenseUrl { get; private set; }
    public string? Address { get; private set; }
    public Guid? CategoryId { get; private set; }
    public AgencyStatus Status { get; private set; } = AgencyStatus.PendingReview;
    public string? RejectionReason { get; private set; }

    // Navigation
    public Wallet? Wallet { get; private set; }
    private readonly List<AgencyUser> _users = [];
    public IReadOnlyList<AgencyUser> Users => _users.AsReadOnly();

    private Agency() { } // EF

    public static Agency Create(
        string nameAr, string nameEn,
        string email, string phone,
        string licenseNumber, DateTime licenseExpiry,
        string? licenseUrl, string? address, Guid? categoryId)
    {
        var agency = new Agency
        {
            Id = Guid.NewGuid(),
            NameAr = nameAr,
            NameEn = nameEn,
            Email = email,
            Phone = phone,
            CommercialLicenseNumber = licenseNumber,
            CommercialLicenseExpiry = licenseExpiry,
            CommercialLicenseUrl = licenseUrl,
            Address = address,
            CategoryId = categoryId,
            Status = AgencyStatus.PendingReview
        };

        return agency;
    }

    public void Approve()
    {
        Status = AgencyStatus.Approved;
        RejectionReason = null;
        RaiseDomainEvent(new AgencyApprovedDomainEvent(Id));
    }

    public void Reject(string reason)
    {
        Status = AgencyStatus.Rejected;
        RejectionReason = reason;
        RaiseDomainEvent(new AgencyRejectedDomainEvent(Id, reason));
    }

    public void Suspend(string reason)
    {
        Status = AgencyStatus.Suspended;
        RejectionReason = reason;
    }

    public void AddUser(AgencyUser user) => _users.Add(user);

    public static Agency Register(
        string agencyName,
        string? commercialRegNumber,
        string contactPersonName,
        string email,
        string passwordHash,
        string countryCode,
        string mobileNumber)
    {
        var agency = new Agency
        {
            Id = Guid.NewGuid(),
            NameAr = agencyName,
            NameEn = agencyName,
            Email = email,
            Phone = mobileNumber,
            CommercialLicenseNumber = commercialRegNumber,
            CommercialLicenseExpiry = DateTime.UtcNow.AddYears(1),
            Status = AgencyStatus.PendingReview
        };

        var user = AgencyUser.Create(
            agency.Id,
            contactPersonName, string.Empty,
            contactPersonName, string.Empty,
            email, mobileNumber,
            passwordHash,
            AgencyUserRole.Admin,
            countryCode);

        agency._users.Add(user);
        agency.Wallet = Wallet.Create(agency.Id);

        agency.RaiseDomainEvent(new AgencyRegisteredDomainEvent(agency.Id, email));

        return agency;
    }
}
