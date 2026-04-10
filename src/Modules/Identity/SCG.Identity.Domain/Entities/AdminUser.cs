using SCG.SharedKernel;

namespace SCG.Identity.Domain.Entities;

public sealed class AdminUser : Entity<Guid>
{
    public string FullName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string Role { get; private set; } = "Admin";
    public bool IsActive { get; private set; } = true;
    public DateTime? LastLoginAt { get; private set; }

    private AdminUser() { } // EF

    public static AdminUser Create(string fullName, string email, string passwordHash, string role = "Admin")
    {
        return new AdminUser
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Email = email,
            PasswordHash = passwordHash,
            Role = role
        };
    }

    public void RecordLogin() => LastLoginAt = DateTime.UtcNow;
}
