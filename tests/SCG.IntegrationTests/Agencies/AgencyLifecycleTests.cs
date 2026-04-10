using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SCG.IntegrationTests.Infrastructure;

namespace SCG.IntegrationTests.Agencies;

public sealed class AgencyLifecycleTests : IntegrationTestBase
{
    public AgencyLifecycleTests(ScgWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task FullLifecycle_Register_Approve_GetDetail()
    {
        // Register
        var body = new
        {
            agencyName = "Lifecycle Agency",
            commercialRegNumber = "CR-LC-001",
            contactPersonName = "Ali Hassan",
            email = "lifecycle@agency.com",
            password = "Pass@1234",
            countryCode = "+20",
            mobileNumber = "01055556666"
        };
        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", body);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Login as admin to list and find pending agency
        await LoginAsAdminAsync();
        var listResponse = await Client.GetFromJsonAsync<JsonElement>("/api/agencies?status=PendingReview&pageSize=50");
        var items = listResponse.GetProperty("items").EnumerateArray().ToList();
        var agency = items.First(a => a.GetProperty("email").GetString() == "lifecycle@agency.com");
        var agencyId = Guid.Parse(agency.GetProperty("id").GetString()!);

        // Approve
        var approveResponse = await Client.PutAsJsonAsync($"/api/agencies/{agencyId}/approve", new { });
        approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Get detail
        var detailResponse = await Client.GetAsync($"/api/agencies/{agencyId}");
        detailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await detailResponse.Content.ReadFromJsonAsync<JsonElement>();
        detail.GetProperty("status").GetString().Should().Be("Approved");
    }

    [Fact]
    public async Task RejectAgency_WithReason_CannotLogin()
    {
        // Register
        var body = new
        {
            agencyName = "Reject Agency",
            commercialRegNumber = "CR-RJ-001",
            contactPersonName = "Omar Youssef",
            email = "reject@agency.com",
            password = "Pass@1234",
            countryCode = "+20",
            mobileNumber = "01077778888"
        };
        await Client.PostAsJsonAsync("/api/auth/register", body);

        // Admin rejects
        await LoginAsAdminAsync();
        var listResponse = await Client.GetFromJsonAsync<JsonElement>("/api/agencies?searchTerm=reject@agency.com");
        var agencyId = Guid.Parse(listResponse.GetProperty("items").EnumerateArray()
            .First().GetProperty("id").GetString()!);

        var rejectResponse = await Client.PutAsJsonAsync($"/api/agencies/{agencyId}/reject", new { reason = "Incomplete documents" });
        rejectResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify status
        var detail = await Client.GetFromJsonAsync<JsonElement>($"/api/agencies/{agencyId}");
        detail.GetProperty("status").GetString().Should().Be("Rejected");

        // Attempt login
        ClearAuth();
        var loginBody = new { email = "reject@agency.com", password = "Pass@1234", loginType = "agency" };
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginBody);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAgencies_FilterByStatus_ReturnsPaginated()
    {
        // Register two agencies
        await RegisterAgencyAndGetTokenAsync("filter1@agency.com", "Pass@1234", "Filter Agency 1");
        await RegisterAgencyAndGetTokenAsync("filter2@agency.com", "Pass@1234", "Filter Agency 2");

        // Admin lists pending
        await LoginAsAdminAsync();
        var response = await Client.GetFromJsonAsync<JsonElement>("/api/agencies?status=PendingReview&page=1&pageSize=10");
        var count = response.GetProperty("items").GetArrayLength();
        count.Should().BeGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task GetAgencies_Anonymous_Returns401()
    {
        ClearAuth();
        var response = await Client.GetAsync("/api/agencies");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
