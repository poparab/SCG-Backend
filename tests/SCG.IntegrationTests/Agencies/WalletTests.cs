using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SCG.IntegrationTests.Infrastructure;

namespace SCG.IntegrationTests.Agencies;

public sealed class WalletTests : IntegrationTestBase
{
    public WalletTests(ScgWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CreditWallet_ValidAmount_IncreasesBalance()
    {
        // Arrange — register & approve agency
        var (token, agencyId) = await RegisterApproveAndLoginAgencyAsync("wallet@agency.com");

        // Admin credits wallet
        await LoginAsAdminAsync();
        var creditBody = new { amount = 5000m, referenceNumber = "REF-001", notes = "Initial top-up" };
        var creditResponse = await Client.PostAsJsonAsync($"/api/agencies/{agencyId}/wallet/credit", creditBody);
        creditResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify balance
        var wallet = await Client.GetFromJsonAsync<JsonElement>($"/api/agencies/{agencyId}/wallet");
        wallet.GetProperty("balance").GetDecimal().Should().Be(5000m);
    }

    [Fact]
    public async Task CreditWallet_MultipleTimes_AccumulatesBalance()
    {
        // Arrange
        var (token, agencyId) = await RegisterApproveAndLoginAgencyAsync("wallet-acc@agency.com");

        // Admin credits twice
        await LoginAsAdminAsync();
        await Client.PostAsJsonAsync($"/api/agencies/{agencyId}/wallet/credit",
            new { amount = 1000m, referenceNumber = "REF-A01", notes = "First" });
        await Client.PostAsJsonAsync($"/api/agencies/{agencyId}/wallet/credit",
            new { amount = 2000m, referenceNumber = "REF-A02", notes = "Second" });

        // Verify
        var wallet = await Client.GetFromJsonAsync<JsonElement>($"/api/agencies/{agencyId}/wallet");
        wallet.GetProperty("balance").GetDecimal().Should().Be(3000m);
    }

    [Fact]
    public async Task GetWalletTransactions_ReturnsPagedHistory()
    {
        // Arrange
        var (token, agencyId) = await RegisterApproveAndLoginAgencyAsync("wallet-tx@agency.com");

        await LoginAsAdminAsync();
        await Client.PostAsJsonAsync($"/api/agencies/{agencyId}/wallet/credit",
            new { amount = 500m, referenceNumber = "REF-TX1", notes = "Credit 1" });
        await Client.PostAsJsonAsync($"/api/agencies/{agencyId}/wallet/credit",
            new { amount = 750m, referenceNumber = "REF-TX2", notes = "Credit 2" });

        // Act
        var response = await Client.GetAsync($"/api/agencies/{agencyId}/wallet/transactions?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("items").GetArrayLength().Should().BeGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task CreditWallet_ZeroAmount_Returns400()
    {
        // Arrange
        var (token, agencyId) = await RegisterApproveAndLoginAgencyAsync("wallet-zero@agency.com");
        await LoginAsAdminAsync();

        // Act
        var creditBody = new { amount = 0m, referenceNumber = "REF-ZERO", notes = "Zero" };
        var response = await Client.PostAsJsonAsync($"/api/agencies/{agencyId}/wallet/credit", creditBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreditWallet_Anonymous_Returns401()
    {
        ClearAuth();
        var response = await Client.PostAsJsonAsync("/api/agencies/00000000-0000-0000-0000-000000000001/wallet/credit",
            new { amount = 100m, referenceNumber = "REF-ANON" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
