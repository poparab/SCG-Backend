using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SCG.IntegrationTests.Infrastructure;

namespace SCG.IntegrationTests.Auth;

public sealed class RegisterTests : IntegrationTestBase
{
    public RegisterTests(ScgWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Register_ValidAgency_Returns200AndGuid()
    {
        // Arrange
        var body = new
        {
            agencyName = "Alpha Agency",
            commercialRegNumber = "CR-001",
            contactPersonName = "Ahmed Ali",
            email = "alpha@agency.com",
            password = "Pass@1234",
            countryCode = "+20",
            mobileNumber = "01012345678"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var agencyId = await response.Content.ReadFromJsonAsync<Guid>();
        agencyId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns400()
    {
        // Arrange — register once
        var body = new
        {
            agencyName = "Beta Agency",
            commercialRegNumber = "CR-002",
            contactPersonName = "Sara Mohamed",
            email = "beta@agency.com",
            password = "Pass@1234",
            countryCode = "+20",
            mobileNumber = "01098765432"
        };
        await Client.PostAsJsonAsync("/api/auth/register", body);

        // Act — register again with same email
        var response = await Client.PostAsJsonAsync("/api/auth/register", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadAsStringAsync();
        error.Should().Contain("already exists");
    }
}
