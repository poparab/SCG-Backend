using FluentValidation;

namespace SCG.InquiryManagement.Application.Commands.UpdateTravelerInBatch;

public sealed class UpdateTravelerInBatchCommandValidator : AbstractValidator<UpdateTravelerInBatchCommand>
{
    public UpdateTravelerInBatchCommandValidator()
    {
        RuleFor(x => x.BatchId)
            .NotEmpty().WithMessage("Batch ID is required.");

        RuleFor(x => x.TravelerId)
            .NotEmpty().WithMessage("Traveler ID is required.");

        RuleFor(x => x.FirstNameEn)
            .NotEmpty().WithMessage("First name (English) is required.")
            .MaximumLength(100).WithMessage("First name (English) must not exceed 100 characters.");

        RuleFor(x => x.LastNameEn)
            .NotEmpty().WithMessage("Last name (English) is required.")
            .MaximumLength(100).WithMessage("Last name (English) must not exceed 100 characters.");

        RuleFor(x => x.FirstNameAr)
            .MaximumLength(100).WithMessage("First name (Arabic) must not exceed 100 characters.")
            .When(x => x.FirstNameAr is not null);

        RuleFor(x => x.LastNameAr)
            .MaximumLength(100).WithMessage("Last name (Arabic) must not exceed 100 characters.")
            .When(x => x.LastNameAr is not null);

        RuleFor(x => x.PassportNumber)
            .NotEmpty().WithMessage("Passport number is required.")
            .Matches(@"^[A-Za-z0-9]{5,20}$").WithMessage("Passport number must be 5–20 alphanumeric characters.");

        RuleFor(x => x.NationalityCode)
            .NotEmpty().WithMessage("Nationality code is required.")
            .Length(2, 3).WithMessage("Nationality code must be 2 or 3 characters.");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required.")
            .LessThan(DateTime.UtcNow.Date).WithMessage("Date of birth must be in the past.");

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Gender value is invalid.");

        RuleFor(x => x.TravelDate)
            .NotEmpty().WithMessage("Travel date is required.")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Travel date cannot be in the past.");

        RuleFor(x => x.PassportExpiry)
            .NotEmpty().WithMessage("Passport expiry date is required.")
            .GreaterThan(DateTime.UtcNow.Date).WithMessage("Passport must not be expired.");

        RuleFor(x => x.DepartureCountry)
            .NotEmpty().WithMessage("Departure country is required.")
            .MaximumLength(100).WithMessage("Departure country must not exceed 100 characters.");

        RuleFor(x => x.PurposeOfTravel)
            .NotEmpty().WithMessage("Purpose of travel is required.")
            .MaximumLength(200).WithMessage("Purpose of travel must not exceed 200 characters.");

        RuleFor(x => x.FlightNumber)
            .MaximumLength(20).WithMessage("Flight number must not exceed 20 characters.")
            .When(x => x.FlightNumber is not null);

        RuleFor(x => x.ArrivalAirport)
            .MaximumLength(100).WithMessage("Arrival airport must not exceed 100 characters.")
            .When(x => x.ArrivalAirport is not null);

        RuleFor(x => x.TransitCountries)
            .MaximumLength(500).WithMessage("Transit countries must not exceed 500 characters.")
            .When(x => x.TransitCountries is not null);
    }
}
