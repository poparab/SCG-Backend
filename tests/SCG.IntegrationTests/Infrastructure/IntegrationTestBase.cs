using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Respawn;

namespace SCG.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IClassFixture<ScgWebApplicationFactory>, IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected readonly ScgWebApplicationFactory Factory;
    private Respawner? _respawner;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    protected IntegrationTestBase(ScgWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        _respawner = await Respawner.CreateAsync(Factory.ConnectionString, new RespawnerOptions
        {
            SchemasToInclude = ["agency", "identity", "inquiry", "rules", "notification"],
            DbAdapter = DbAdapter.SqlServer
        });

        await _respawner.ResetAsync(Factory.ConnectionString);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // --------- Auth Helpers ---------

    protected async Task<string> RegisterAgencyAndGetTokenAsync(
        string email = "test@agency.com",
        string password = "Test@1234",
        string agencyName = "Test Agency")
    {
        // Register
        var registerBody = new
        {
            agencyName,
            commercialRegNumber = "CR-12345",
            contactPersonName = "John Doe",
            email,
            password,
            countryCode = "+20",
            mobileNumber = "01012345678"
        };
        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", registerBody);
        registerResponse.EnsureSuccessStatusCode();

        return email;
    }

    protected async Task<(string Token, Guid AgencyId)> RegisterApproveAndLoginAgencyAsync(
        string email = "test@agency.com",
        string password = "Test@1234")
    {
        await RegisterAgencyAndGetTokenAsync(email, password);

        // Login as admin first to approve the agency
        var adminToken = await LoginAsAdminAsync();

        // Find the agency
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var agenciesResponse = await Client.GetFromJsonAsync<JsonElement>("/api/agencies?searchTerm=" + email);
        var agencies = agenciesResponse.GetProperty("items").EnumerateArray().ToList();
        var agencyId = Guid.Parse(agencies[0].GetProperty("id").GetString()!);

        // Approve it
        await Client.PutAsJsonAsync($"/api/agencies/{agencyId}/approve", new { });

        // Login as agency
        Client.DefaultRequestHeaders.Authorization = null;
        var loginBody = new { email, password, loginType = "agency" };
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginBody);
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var token = loginResult.GetProperty("token").GetString()!;

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return (token, agencyId);
    }

    protected async Task<string> LoginAsAdminAsync()
    {
        var loginBody = new { email = "admin@scg.gov.eg", password = "Admin@1234", loginType = "admin" };
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginBody);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        var token = result.GetProperty("token").GetString()!;
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return token;
    }

    protected void SetAuthToken(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    protected void ClearAuth()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }

    // --------- Setup Helpers ---------

    protected async Task<Guid> CreateNationalityAsync(string code, string nameEn, decimal fee = 100m)
    {
        var adminToken = await LoginAsAdminAsync();
        var body = new
        {
            code,
            nameAr = nameEn,
            nameEn,
            requiresInquiry = true,
            defaultFee = fee
        };
        var response = await Client.PostAsJsonAsync("/api/nationalities", body);
        response.EnsureSuccessStatusCode();
        var id = await response.Content.ReadFromJsonAsync<Guid>();
        return id;
    }

    protected async Task CreditWalletAsync(Guid agencyId, decimal amount)
    {
        var adminToken = await LoginAsAdminAsync();
        var body = new { amount, referenceNumber = $"REF-{Guid.NewGuid():N}"[..20], notes = "Test credit" };
        var response = await Client.PostAsJsonAsync($"/api/agencies/{agencyId}/wallet/credit", body);
        response.EnsureSuccessStatusCode();
    }
}
