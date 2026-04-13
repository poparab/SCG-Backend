using System.Net;
using System.Net.Http.Json;
using System.Globalization;
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
        using var creditBody = CreateCreditRequest(5000m, reference: "REF-001", notes: "Initial top-up");
        var creditResponse = await Client.PostAsync($"/api/agencies/{agencyId}/wallet/credit", creditBody);
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
        using var firstCredit = CreateCreditRequest(1000m, reference: "REF-A01", notes: "First");
        using var secondCredit = CreateCreditRequest(2000m, paymentMethod: "BankTransfer", reference: "REF-A02", notes: "Second");
        await Client.PostAsync($"/api/agencies/{agencyId}/wallet/credit", firstCredit);
        await Client.PostAsync($"/api/agencies/{agencyId}/wallet/credit", secondCredit);

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
        using var firstCredit = CreateCreditRequest(500m, reference: "REF-TX1", notes: "Credit 1");
        using var secondCredit = CreateCreditRequest(750m, reference: "REF-TX2", notes: "Credit 2");
        await Client.PostAsync($"/api/agencies/{agencyId}/wallet/credit", firstCredit);
        await Client.PostAsync($"/api/agencies/{agencyId}/wallet/credit", secondCredit);

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
        using var creditBody = CreateCreditRequest(0m, reference: "REF-ZERO", notes: "Zero");
        var response = await Client.PostAsync($"/api/agencies/{agencyId}/wallet/credit", creditBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreditWallet_WithoutReference_Returns200()
    {
        // Arrange
        var (token, agencyId) = await RegisterApproveAndLoginAgencyAsync("wallet-noref@agency.com");
        await LoginAsAdminAsync();

        // Act
        using var creditBody = CreateCreditRequest(900m, notes: "No reference provided");
        var response = await Client.PostAsync($"/api/agencies/{agencyId}/wallet/credit", creditBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var wallet = await Client.GetFromJsonAsync<JsonElement>($"/api/agencies/{agencyId}/wallet");
        wallet.GetProperty("balance").GetDecimal().Should().Be(900m);
    }

    [Fact]
    public async Task CreditWallet_Anonymous_Returns401()
    {
        ClearAuth();
        using var creditBody = CreateCreditRequest(100m, reference: "REF-ANON");
        var response = await Client.PostAsync("/api/agencies/00000000-0000-0000-0000-000000000001/wallet/credit", creditBody);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static MultipartFormDataContent CreateCreditRequest(
        decimal amount,
        string paymentMethod = "Cash",
        string? reference = null,
        string? notes = null)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(amount.ToString(CultureInfo.InvariantCulture)), "Amount");
        content.Add(new StringContent(paymentMethod), "PaymentMethod");

        if (!string.IsNullOrWhiteSpace(reference))
        {
            content.Add(new StringContent(reference), "Reference");
        }

        if (!string.IsNullOrWhiteSpace(notes))
        {
            content.Add(new StringContent(notes), "Notes");
        }

        return content;
    }
}
