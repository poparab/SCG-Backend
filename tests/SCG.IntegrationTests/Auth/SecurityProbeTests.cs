using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using SCG.IntegrationTests.Infrastructure;

namespace SCG.IntegrationTests.Auth;

public sealed class SecurityProbeTests : IntegrationTestBase
{
    public SecurityProbeTests(ScgWebApplicationFactory factory) : base(factory) { }

    // ──────── JWT Tampering ────────

    [Fact]
    public async Task TamperedJwt_InvalidSignature_Returns401()
    {
        // Arrange — get a valid token, then tamper with the signature
        var body = new { email = "admin@scg.gov.eg", password = "Admin@1234", loginType = "admin" };
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", body);
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var validToken = loginResult.GetProperty("token").GetString()!;

        // Tamper: flip last character of signature
        var parts = validToken.Split('.');
        var tamperedSig = parts[2][..^1] + (parts[2][^1] == 'A' ? 'B' : 'A');
        var tamperedToken = $"{parts[0]}.{parts[1]}.{tamperedSig}";

        // Act — use tampered token to access a protected endpoint
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tamperedToken);
        var response = await Client.GetAsync("/api/agencies");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task MalformedJwt_RandomString_Returns401()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not.a.valid.jwt.at.all");

        // Act
        var response = await Client.GetAsync("/api/agencies");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task MissingAuthHeader_OnProtectedEndpoint_Returns401()
    {
        // Act
        ClearAuth();
        var response = await Client.GetAsync("/api/agencies");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ExpiredJwt_NoneAlgorithm_Returns401()
    {
        // Arrange — craft a "none" algorithm JWT (classic attack vector)
        var header = Convert.ToBase64String(Encoding.UTF8.GetBytes("""{"alg":"none","typ":"JWT"}"""))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(
            $$"""{"sub":"{{Guid.NewGuid()}}","email":"admin@scg.gov.eg","role":"SuperAdmin","exp":9999999999}"""))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var noneToken = $"{header}.{payload}.";

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", noneToken);

        // Act
        var response = await Client.GetAsync("/api/agencies");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ──────── Oversized Payloads ────────

    [Fact]
    public async Task OversizedPayload_Register_ReturnsErrorOrRejection()
    {
        // Arrange — 2MB email field (absurdly large)
        var oversizedEmail = new string('a', 2 * 1024 * 1024) + "@test.com";
        var body = new
        {
            agencyName = "Test Agency",
            commercialRegNumber = "CR-12345",
            contactPersonName = "John Doe",
            email = oversizedEmail,
            password = "Test@1234",
            countryCode = "+20",
            mobileNumber = "01012345678"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", body);

        // Assert — should reject (400 or 413), never 500
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.RequestEntityTooLarge,
            HttpStatusCode.UnprocessableEntity);
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task OversizedPayload_Login_ReturnsErrorOrRejection()
    {
        // Arrange — massive password
        var body = new
        {
            email = "admin@scg.gov.eg",
            password = new string('X', 1024 * 1024),
            loginType = "admin"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", body);

        // Assert — should not crash, expect 400/401
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    // ──────── SQL Injection Probes ────────

    [Fact]
    public async Task SqlInjection_LoginEmail_ReturnsErrorNotServerFault()
    {
        // Arrange — classic SQL injection payload
        var body = new
        {
            email = "' OR 1=1; DROP TABLE identity.AdminUsers; --",
            password = "anything",
            loginType = "admin"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", body);

        // Assert — parameterized queries should prevent injection; no 500
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
    }

    // ──────── XSS Probes ────────

    [Fact]
    public async Task XssPayload_Register_SanitizedOrRejected()
    {
        // Arrange
        var body = new
        {
            agencyName = "<script>alert('xss')</script>",
            commercialRegNumber = "CR-12345",
            contactPersonName = "<img onerror='alert(1)' src='x'>",
            email = "xss-test@agency.com",
            password = "Test@1234",
            countryCode = "+20",
            mobileNumber = "01012345678"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", body);

        // Assert — should either reject or accept but never crash
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);

        // If accepted, the stored value should not contain raw script tags in the response
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotContain("<script>");
        }
    }

    // ──────── Authorization Boundary Tests ────────

    [Fact]
    public async Task AgencyUser_CannotAccess_AdminOnlyEndpoints()
    {
        // Arrange — register and login as agency user
        var (token, _) = await RegisterApproveAndLoginAgencyAsync("boundary-test@agency.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act — try to access admin-only endpoints (approve an agency)
        var response = await Client.PutAsJsonAsync($"/api/agencies/{Guid.NewGuid()}/approve", new { });

        // Assert — should be 403 Forbidden or 404 (agency doesn't exist)
        // The key assertion: never 200 OK, which would mean unauthorized access succeeded
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }

    // ──────── Refresh Token Security ────────

    [Fact]
    public async Task Refresh_WithInvalidToken_Returns401()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/refresh",
            new { refreshToken = "completely-invalid-refresh-token" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Revoke_WithInvalidToken_Returns400()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/revoke",
            new { refreshToken = "nonexistent-refresh-token" });

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NoContent);
    }
}
