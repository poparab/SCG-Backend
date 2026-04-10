using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SCG.IntegrationTests.Infrastructure;

namespace SCG.IntegrationTests.Batches;

public sealed class BatchLifecycleTests : IntegrationTestBase
{
    public BatchLifecycleTests(ScgWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task FullBatchLifecycle_Create_AddTravelers_Submit()
    {
        // Setup: nationality + approved agency with wallet
        var nationalityId = await CreateNationalityAsync("SY", "Syria", 100m);
        var (agencyToken, agencyId) = await RegisterApproveAndLoginAgencyAsync("batch@agency.com");

        // Credit wallet (as admin)
        await CreditWalletAsync(agencyId, 5000m);

        // Create batch (as agency)
        SetAuthToken(agencyToken);
        var batchBody = new
        {
            agencyId,
            name = "Test Batch 1",
            inquiryTypeId = (Guid?)null,
            nationalityCode = "SY"
        };
        var createResponse = await Client.PostAsJsonAsync("/api/batches", batchBody);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var batchId = await createResponse.Content.ReadFromJsonAsync<Guid>();
        batchId.Should().NotBeEmpty();

        // Add traveler 1
        var traveler1 = new
        {
            firstNameEn = "Ahmad",
            lastNameEn = "Khalil",
            passportNumber = "SY123456",
            nationalityCode = "SY",
            dateOfBirth = "1990-05-15",
            gender = 0, // Male
            travelDate = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd"),
            arrivalAirport = "CAI"
        };
        var addResponse1 = await Client.PostAsJsonAsync($"/api/batches/{batchId}/travelers", traveler1);
        addResponse1.StatusCode.Should().Be(HttpStatusCode.OK);

        // Add traveler 2
        var traveler2 = new
        {
            firstNameEn = "Fatima",
            lastNameEn = "Hassan",
            passportNumber = "SY789012",
            nationalityCode = "SY",
            dateOfBirth = "1995-08-20",
            gender = 1, // Female
            travelDate = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd"),
            arrivalAirport = "HRG"
        };
        var addResponse2 = await Client.PostAsJsonAsync($"/api/batches/{batchId}/travelers", traveler2);
        addResponse2.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify batch detail shows 2 travelers
        var detail = await Client.GetFromJsonAsync<JsonElement>($"/api/batches/{batchId}");
        detail.GetProperty("travelers").GetArrayLength().Should().Be(2);

        // Submit batch
        var submitResponse = await Client.PostAsJsonAsync($"/api/batches/{batchId}/submit", new { });
        submitResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateBatch_AsAgency_Returns201()
    {
        // Setup
        await CreateNationalityAsync("IQ", "Iraq", 80m);
        var (agencyToken, agencyId) = await RegisterApproveAndLoginAgencyAsync("batch-create@agency.com");
        SetAuthToken(agencyToken);

        // Act
        var body = new
        {
            agencyId,
            name = "Quick Batch",
            inquiryTypeId = (Guid?)null,
            nationalityCode = "IQ"
        };
        var response = await Client.PostAsJsonAsync("/api/batches", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task UpdateTraveler_InDraftBatch_Returns204()
    {
        // Setup
        await CreateNationalityAsync("LB", "Lebanon", 100m);
        var (agencyToken, agencyId) = await RegisterApproveAndLoginAgencyAsync("batch-update@agency.com");
        SetAuthToken(agencyToken);

        // Create batch + traveler
        var batchBody = new { agencyId, name = "Update Batch", nationalityCode = "LB" };
        var createResp = await Client.PostAsJsonAsync("/api/batches", batchBody);
        var batchId = await createResp.Content.ReadFromJsonAsync<Guid>();

        var traveler = new
        {
            firstNameEn = "Nour",
            lastNameEn = "Ahmad",
            passportNumber = "LB111222",
            nationalityCode = "LB",
            dateOfBirth = "1988-03-10",
            gender = 1,
            travelDate = DateTime.UtcNow.AddDays(14).ToString("yyyy-MM-dd")
        };
        var addResp = await Client.PostAsJsonAsync($"/api/batches/{batchId}/travelers", traveler);
        var travelerId = await addResp.Content.ReadFromJsonAsync<Guid>();

        // Act — update
        var updateBody = new
        {
            firstNameEn = "Nour Updated",
            lastNameEn = "Ahmad",
            passportNumber = "LB111222",
            nationalityCode = "LB",
            dateOfBirth = "1988-03-10",
            gender = 1,
            travelDate = DateTime.UtcNow.AddDays(21).ToString("yyyy-MM-dd")
        };
        var response = await Client.PutAsJsonAsync($"/api/batches/{batchId}/travelers/{travelerId}", updateBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveTraveler_FromDraftBatch_Returns204()
    {
        // Setup
        await CreateNationalityAsync("SD", "Sudan", 90m);
        var (agencyToken, agencyId) = await RegisterApproveAndLoginAgencyAsync("batch-remove@agency.com");
        SetAuthToken(agencyToken);

        // Create batch + traveler
        var batchBody = new { agencyId, name = "Remove Batch", nationalityCode = "SD" };
        var createResp = await Client.PostAsJsonAsync("/api/batches", batchBody);
        var batchId = await createResp.Content.ReadFromJsonAsync<Guid>();

        var traveler = new
        {
            firstNameEn = "Salim",
            lastNameEn = "Osman",
            passportNumber = "SD333444",
            nationalityCode = "SD",
            dateOfBirth = "1992-07-01",
            gender = 0,
            travelDate = DateTime.UtcNow.AddDays(10).ToString("yyyy-MM-dd")
        };
        var addResp = await Client.PostAsJsonAsync($"/api/batches/{batchId}/travelers", traveler);
        var travelerId = await addResp.Content.ReadFromJsonAsync<Guid>();

        // Act
        var response = await Client.DeleteAsync($"/api/batches/{batchId}/travelers/{travelerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetBatches_ReturnsPagedList()
    {
        // Setup
        await CreateNationalityAsync("YE", "Yemen", 75m);
        var (agencyToken, agencyId) = await RegisterApproveAndLoginAgencyAsync("batch-list@agency.com");
        SetAuthToken(agencyToken);

        // Create two batches
        await Client.PostAsJsonAsync("/api/batches", new { agencyId, name = "Batch A", nationalityCode = "YE" });
        await Client.PostAsJsonAsync("/api/batches", new { agencyId, name = "Batch B", nationalityCode = "YE" });

        // Act
        var response = await Client.GetAsync($"/api/batches?agencyId={agencyId}&page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("items").GetArrayLength().Should().BeGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task CreateBatch_Anonymous_Returns401()
    {
        ClearAuth();
        var body = new { agencyId = Guid.NewGuid(), name = "Anon Batch", nationalityCode = "XX" };
        var response = await Client.PostAsJsonAsync("/api/batches", body);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
