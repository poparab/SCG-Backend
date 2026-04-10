using Microsoft.AspNetCore.Http;

namespace SCG.API.Contracts.Agencies;

public sealed class CreditWalletRequest
{
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = default!;
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public IFormFile? Evidence { get; set; }
}
