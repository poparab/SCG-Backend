using SCG.InquiryManagement.Domain.Enums;

namespace SCG.API.Contracts.Batches;

public sealed record CreateBatchRequest(
    Guid AgencyId,
    string Name,
    string? InquiryTypeId,
    string? Notes);

public sealed class AddTravelerRequest
{
    public string FirstNameEn { get; set; } = string.Empty;
    public string LastNameEn { get; set; } = string.Empty;
    public string? FirstNameAr { get; set; }
    public string? LastNameAr { get; set; }
    public string PassportNumber { get; set; } = string.Empty;
    public string NationalityCode { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public TravelerGender Gender { get; set; }
    public DateTime TravelDate { get; set; }
    public string? ArrivalAirport { get; set; }
    public string? TransitCountries { get; set; }
    public DateTime PassportExpiry { get; set; }
    public string DepartureCountry { get; set; } = string.Empty;
    public string PurposeOfTravel { get; set; } = string.Empty;
    public string? FlightNumber { get; set; }
    public IFormFile? PassportImageDocument { get; set; }
    public IFormFile? TicketImageDocument { get; set; }
}

public sealed class UpdateTravelerRequest
{
    public string FirstNameEn { get; set; } = string.Empty;
    public string LastNameEn { get; set; } = string.Empty;
    public string? FirstNameAr { get; set; }
    public string? LastNameAr { get; set; }
    public string PassportNumber { get; set; } = string.Empty;
    public string NationalityCode { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public TravelerGender Gender { get; set; }
    public DateTime TravelDate { get; set; }
    public string? ArrivalAirport { get; set; }
    public string? TransitCountries { get; set; }
    public DateTime PassportExpiry { get; set; }
    public string DepartureCountry { get; set; } = string.Empty;
    public string PurposeOfTravel { get; set; } = string.Empty;
    public string? FlightNumber { get; set; }
    public IFormFile? PassportImageDocument { get; set; }
    public IFormFile? TicketImageDocument { get; set; }
}

public sealed record SubmitBatchRequest(Guid SubmittedByUserId);
