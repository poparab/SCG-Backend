namespace SCG.API.Contracts.AdminUsers;

public sealed record CreateAdminUserRequest(
    string FullName,
    string Email,
    string Password,
    string Role);
