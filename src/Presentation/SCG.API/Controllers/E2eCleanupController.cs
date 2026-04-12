using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace SCG.API.Controllers;

/// <summary>
/// Development-only endpoint to clean up E2E test data.
/// Deletes agencies, batches, inquiries, and wallet data created by E2E tests
/// (identified by @test.com emails). Never available in production.
/// </summary>
[ApiController]
[Route("api/e2e")]
[Authorize(Roles = "SuperAdmin")]
public class E2eCleanupController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<E2eCleanupController> _logger;

    public E2eCleanupController(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger<E2eCleanupController> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
    }

    [HttpPost("cleanup")]
    public async Task<IActionResult> Cleanup(CancellationToken ct)
    {
        if (_environment.IsProduction())
        {
            return NotFound();
        }

        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            return StatusCode(500, new { error = "No connection string configured." });
        }

        try
        {
            var deleted = new Dictionary<string, int>();

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(ct);

            // FK chain: BatchTravelers → (Inquiries, Batches), Inquiries → Batches,
            //           Batches → Agencies, WalletTransactions → Wallets → Agencies,
            //           AgencyUsers → Agencies
            // Delete order: leaf tables first, then parents.

            // 1. BatchTravelers (FK to both Inquiries and Batches)
            var travelerCount = await ExecuteSqlAsync(connection, @"
                DELETE bt FROM inquiry.BatchTravelers bt
                INNER JOIN inquiry.Batches b ON bt.BatchId = b.Id
                INNER JOIN agency.Agencies a ON b.AgencyId = a.Id
                WHERE a.Email LIKE '%@test.com'", ct);
            deleted["batchTravelers"] = travelerCount;

            // 2. Inquiries (FK to Batches)
            var inquiryCount = await ExecuteSqlAsync(connection, @"
                DELETE i FROM inquiry.Inquiries i
                INNER JOIN inquiry.Batches b ON i.BatchId = b.Id
                INNER JOIN agency.Agencies a ON b.AgencyId = a.Id
                WHERE a.Email LIKE '%@test.com'", ct);
            deleted["inquiries"] = inquiryCount;

            // 3. Batches
            var batchCount = await ExecuteSqlAsync(connection, @"
                DELETE b FROM inquiry.Batches b
                INNER JOIN agency.Agencies a ON b.AgencyId = a.Id
                WHERE a.Email LIKE '%@test.com'", ct);
            deleted["batches"] = batchCount;

            // 4. WalletTransactions (FK to Wallets)
            var txCount = await ExecuteSqlAsync(connection, @"
                DELETE wt FROM agency.WalletTransactions wt
                INNER JOIN agency.Wallets w ON wt.WalletId = w.Id
                INNER JOIN agency.Agencies a ON w.AgencyId = a.Id
                WHERE a.Email LIKE '%@test.com'", ct);
            deleted["walletTransactions"] = txCount;

            // 5. Wallets (FK to Agencies)
            var walletCount = await ExecuteSqlAsync(connection, @"
                DELETE w FROM agency.Wallets w
                INNER JOIN agency.Agencies a ON w.AgencyId = a.Id
                WHERE a.Email LIKE '%@test.com'", ct);
            deleted["wallets"] = walletCount;

            // 6. AgencyUsers (FK to Agencies)
            var userCount = await ExecuteSqlAsync(connection, @"
                DELETE au FROM agency.AgencyUsers au
                INNER JOIN agency.Agencies a ON au.AgencyId = a.Id
                WHERE a.Email LIKE '%@test.com'", ct);
            deleted["agencyUsers"] = userCount;

            // 7. Agencies
            var agencyCount = await ExecuteSqlAsync(connection, @"
                DELETE FROM agency.Agencies WHERE Email LIKE '%@test.com'", ct);
            deleted["agencies"] = agencyCount;

            // 8. Delete expired refresh tokens
            var tokenCount = await ExecuteSqlAsync(connection, @"
                DELETE FROM [identity].RefreshTokens WHERE ExpiresAt < GETUTCDATE()", ct);
            deleted["expiredRefreshTokens"] = tokenCount;

            _logger.LogInformation("E2E cleanup completed: {@Deleted}", deleted);

            return Ok(deleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "E2E cleanup failed");
            return StatusCode(500, new { error = ex.Message, detail = ex.InnerException?.Message });
        }
    }

    private static async Task<int> ExecuteSqlAsync(SqlConnection connection, string sql, CancellationToken ct)
    {
        await using var cmd = new SqlCommand(sql, connection);
        cmd.CommandTimeout = 30;
        return await cmd.ExecuteNonQueryAsync(ct);
    }
}
