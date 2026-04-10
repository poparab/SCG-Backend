using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SCG.IntegrationTests.Infrastructure;

namespace SCG.IntegrationTests.Auth;

public sealed class LoginTests : IntegrationTestBase
{
    public LoginTests(ScgWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Login_AdminWithValidCredentials_ReturnsTokenAndSuperAdminRole()
    {
        // Act
        var body = new { email = "admin@scg.gov.eg", password = "Admin@1234", loginType = "admin" };
        var response = await Client.PostAsJsonAsync("/api/auth/login", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
        result.GetProperty("role").GetString().Should().Be("SuperAdmin");
    }

    [Fact]
    public async Task Login_ApprovedAgency_ReturnsTokenWithAgencyId()
    {
        // Arrange — register, approve, login
        var (token, agencyId) = await RegisterApproveAndLoginAgencyAsync("agency-login@test.com");

        // Assert
        token.Should().NotBeNullOrEmpty();
        agencyId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Login_UnapprovedAgency_Returns400PendingApproval()
    {
        // Arrange — register but don't approve
        await RegisterAgencyAndGetTokenAsync("unapproved@test.com", "Pass@1234");
        ClearAuth();

        // Act
        var body = new { email = "unapproved@test.com", password = "Pass@1234", loginType = "agency" };
        var response = await Client.PostAsJsonAsync("/api/auth/login", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadAsStringAsync();
        error.Should().Contain("pending approval");
    }

    [Fact]
    public async Task Login_WrongPassword_Returns400()
    {
        // Act
        var body = new { email = "admin@scg.gov.eg", password = "wrongpassword", loginType = "admin" };
        var response = await Client.PostAsJsonAsync("/api/auth/login", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadAsStringAsync();
        error.Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task Login_NonExistentUser_Returns400()
    {
        // Act
        var body = new { email = "nobody@test.com", password = "Pass@1234", loginType = "agency" };
        var response = await Client.PostAsJsonAsync("/api/auth/login", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadAsStringAsync();
        error.Should().Contain("Invalid credentials");
    }
}
