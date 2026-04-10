using SCG.AgencyManagement.Domain.Enums;
using SCG.SharedKernel;

namespace SCG.AgencyManagement.Domain.Entities;

public sealed class AgencyUser : Entity<Guid>
{
    public Guid AgencyId { get; private set; }
    public string FirstNameAr { get; private set; } = default!;
    public string LastNameAr { get; private set; } = default!;
    public string FirstNameEn { get; private set; } = default!;
    public string LastNameEn { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string Phone { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string? CountryCode { get; private set; }
    public AgencyUserRole Role { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsEmailVerified { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    // Navigation
    public Agency Agency { get; private set; } = default!;

    private AgencyUser() { } // EF

    public static AgencyUser Create(
        Guid agencyId,
        string firstNameAr, string lastNameAr,
        string firstNameEn, string lastNameEn,
        string email, string phone,
        string passwordHash,
        AgencyUserRole role,
        string? countryCode = null)
    {
        return new AgencyUser
        {
            Id = Guid.NewGuid(),
            AgencyId = agencyId,
            FirstNameAr = firstNameAr,
            LastNameAr = lastNameAr,
            FirstNameEn = firstNameEn,
            LastNameEn = lastNameEn,
            Email = email,
            Phone = phone,
            PasswordHash = passwordHash,
            Role = role,
            CountryCode = countryCode
        };
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
    public void VerifyEmail() => IsEmailVerified = true;
    public void RecordLogin() => LastLoginAt = DateTime.UtcNow;
    public void UpdatePasswordHash(string hash) => PasswordHash = hash;
}
