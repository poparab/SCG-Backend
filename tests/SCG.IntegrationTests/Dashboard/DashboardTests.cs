using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SCG.IntegrationTests.Infrastructure;

namespace SCG.IntegrationTests.Dashboard;

public sealed class DashboardTests : IntegrationTestBase
{
    public DashboardTests(ScgWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task AdminDashboard_AsAdmin_Returns200WithKpis()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/dashboard/admin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("totalAgencies").GetInt32().Should().BeGreaterOrEqualTo(0);
        result.GetProperty("totalInquiries").GetInt32().Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task AgencyDashboard_AsAgency_Returns200WithKpis()
    {
        // Arrange
        var (token, agencyId) = await RegisterApproveAndLoginAgencyAsync("dashboard@agency.com");
        SetAuthToken(token);

        // Act
        var response = await Client.GetAsync($"/api/dashboard/agency/{agencyId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("walletBalance").GetDecimal().Should().BeGreaterOrEqualTo(0);
        result.GetProperty("totalBatches").GetInt32().Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task AdminDashboard_Anonymous_Returns401()
    {
        ClearAuth();
        var response = await Client.GetAsync("/api/dashboard/admin");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminDashboard_WithData_ReflectsRegisteredAgencies()
    {
        // Arrange — register some agencies
        await RegisterAgencyAndGetTokenAsync("dash-a1@agency.com", "Pass@1234", "Dash Agency 1");
        await RegisterAgencyAndGetTokenAsync("dash-a2@agency.com", "Pass@1234", "Dash Agency 2");

        await LoginAsAdminAsync();

        // Act
        var result = await Client.GetFromJsonAsync<JsonElement>("/api/dashboard/admin");

        // Assert
        result.GetProperty("totalAgencies").GetInt32().Should().BeGreaterOrEqualTo(2);
        result.GetProperty("pendingAgencies").GetInt32().Should().BeGreaterOrEqualTo(2);
    }
}
