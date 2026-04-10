using SCG.InquiryManagement.Domain.Enums;
using SCG.SharedKernel;

namespace SCG.InquiryManagement.Domain.Entities;

public sealed class BatchTraveler : Entity<Guid>
{
    public Guid BatchId { get; private set; }
    public string FirstNameEn { get; private set; } = default!;
    public string LastNameEn { get; private set; } = default!;
    public string? FirstNameAr { get; private set; }
    public string? LastNameAr { get; private set; }
    public string PassportNumber { get; private set; } = default!;
    public string NationalityCode { get; private set; } = default!;
    public DateTime DateOfBirth { get; private set; }
    public TravelerGender Gender { get; private set; }
    public DateTime TravelDate { get; private set; }
    public string? ArrivalAirport { get; private set; }
    public string? TransitCountries { get; private set; } // JSON array
    public DateTime PassportExpiry { get; private set; }
    public string DepartureCountry { get; private set; } = default!;
    public string PurposeOfTravel { get; private set; } = default!;
    public string? FlightNumber { get; private set; }
    public int RowIndex { get; private set; }

    // Linked inquiry (created after payment)
    public Guid? InquiryId { get; private set; }
    public Inquiry? Inquiry { get; private set; }

    // Navigation
    public Batch Batch { get; private set; } = default!;

    private BatchTraveler() { } // EF

    public static BatchTraveler Create(
        Guid batchId, int rowIndex,
        string firstNameEn, string lastNameEn,
        string? firstNameAr, string? lastNameAr,
        string passportNumber, string nationalityCode,
        DateTime dateOfBirth, TravelerGender gender,
        DateTime travelDate, string? arrivalAirport,
        string? transitCountries,
        DateTime passportExpiry, string departureCountry,
        string purposeOfTravel, string? flightNumber)
    {
        return new BatchTraveler
        {
            Id = Guid.NewGuid(),
            BatchId = batchId,
            RowIndex = rowIndex,
            FirstNameEn = firstNameEn,
            LastNameEn = lastNameEn,
            FirstNameAr = firstNameAr,
            LastNameAr = lastNameAr,
            PassportNumber = passportNumber,
            NationalityCode = nationalityCode,
            DateOfBirth = dateOfBirth,
            Gender = gender,
            TravelDate = travelDate,
            ArrivalAirport = arrivalAirport,
            TransitCountries = transitCountries,
            PassportExpiry = passportExpiry,
            DepartureCountry = departureCountry,
            PurposeOfTravel = purposeOfTravel,
            FlightNumber = flightNumber
        };
    }

    public void LinkInquiry(Guid inquiryId) => InquiryId = inquiryId;

    public void Update(
        string firstNameEn, string lastNameEn,
        string? firstNameAr, string? lastNameAr,
        string passportNumber, string nationalityCode,
        DateTime dateOfBirth, TravelerGender gender,
        DateTime travelDate, string? arrivalAirport,
        string? transitCountries,
        DateTime passportExpiry, string departureCountry,
        string purposeOfTravel, string? flightNumber)
    {
        FirstNameEn = firstNameEn;
        LastNameEn = lastNameEn;
        FirstNameAr = firstNameAr;
        LastNameAr = lastNameAr;
        PassportNumber = passportNumber;
        NationalityCode = nationalityCode;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        TravelDate = travelDate;
        ArrivalAirport = arrivalAirport;
        TransitCountries = transitCountries;
        PassportExpiry = passportExpiry;
        DepartureCountry = departureCountry;
        PurposeOfTravel = purposeOfTravel;
        FlightNumber = flightNumber;
    }
}
