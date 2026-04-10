using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SCG.IntegrationTests.Infrastructure;

namespace SCG.IntegrationTests.Nationalities;

public sealed class NationalityCrudTests : IntegrationTestBase
{
    public NationalityCrudTests(ScgWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task AddNationality_AsAdmin_Returns201WithId()
    {
        // Arrange
        await LoginAsAdminAsync();
        var body = new
        {
            code = "SY",
            nameAr = "سوريا",
            nameEn = "Syria",
            requiresInquiry = true,
            defaultFee = 150m
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/nationalities", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await response.Content.ReadFromJsonAsync<Guid>();
        id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AddNationality_DuplicateCode_Returns400()
    {
        // Arrange
        await CreateNationalityAsync("IQ", "Iraq", 100m);

        // Act — same code again
        var body = new
        {
            code = "IQ",
            nameAr = "العراق",
            nameEn = "Iraq",
            requiresInquiry = true,
            defaultFee = 100m
        };
        var response = await Client.PostAsJsonAsync("/api/nationalities", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadAsStringAsync();
        error.Should().Contain("already exists");
    }

    [Fact]
    public async Task GetNationalities_ReturnsPagedList()
    {
        // Arrange
        await CreateNationalityAsync("AF", "Afghanistan", 100m);
        await CreateNationalityAsync("PK", "Pakistan", 120m);

        // Act
        var response = await Client.GetAsync("/api/nationalities?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("items").GetArrayLength().Should().BeGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task GetNationalityById_ExistingId_Returns200()
    {
        // Arrange
        var id = await CreateNationalityAsync("YE", "Yemen", 80m);

        // Act
        var response = await Client.GetAsync($"/api/nationalities/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("code").GetString().Should().Be("YE");
        result.GetProperty("nameEn").GetString().Should().Be("Yemen");
    }

    [Fact]
    public async Task UpdateFee_ExistingNationality_Returns200()
    {
        // Arrange
        var id = await CreateNationalityAsync("LB", "Lebanon", 100m);

        // Act
        var body = new
        {
            newFee = 200m,
            effectiveFrom = DateTime.UtcNow.ToString("o"),
            effectiveTo = (string?)null
        };
        var response = await Client.PutAsJsonAsync($"/api/nationalities/{id}/fee", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ToggleInquiry_ExistingNationality_Returns200()
    {
        // Arrange
        var id = await CreateNationalityAsync("SD", "Sudan", 90m);

        // Act — disable inquiry
        var response = await Client.PutAsJsonAsync($"/api/nationalities/{id}/toggle-inquiry", new { requiresInquiry = false });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify — GET should show requiresInquiry = false
        var detail = await Client.GetFromJsonAsync<JsonElement>($"/api/nationalities/{id}");
        detail.GetProperty("requiresInquiry").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task GetPricingList_ReturnsActivePricing()
    {
        // Arrange
        await CreateNationalityAsync("LY", "Libya", 150m);

        // Act
        var response = await Client.GetAsync("/api/nationalities/pricing?activeOnly=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetNationalities_Anonymous_Returns401()
    {
        // Arrange
        ClearAuth();

        // Act
        var response = await Client.GetAsync("/api/nationalities");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
