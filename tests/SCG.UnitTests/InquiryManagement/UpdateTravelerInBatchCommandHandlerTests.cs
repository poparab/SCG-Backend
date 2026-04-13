using SCG.InquiryManagement.Application.Abstractions;
using SCG.InquiryManagement.Application.Commands.UpdateTravelerInBatch;
using SCG.InquiryManagement.Domain.Entities;
using SCG.InquiryManagement.Domain.Enums;

namespace SCG.UnitTests.InquiryManagement;

public sealed class UpdateTravelerInBatchCommandHandlerTests
{
    private readonly IBatchRepository _batchRepository = Substitute.For<IBatchRepository>();
    private readonly UpdateTravelerInBatchCommandHandler _sut;

    public UpdateTravelerInBatchCommandHandlerTests()
    {
        _sut = new UpdateTravelerInBatchCommandHandler(_batchRepository);
    }

    private static Batch CreateBatchWithTraveler(out Guid travelerId)
    {
        var batch = Batch.Create(Guid.NewGuid(), "Test Batch", Guid.NewGuid(), null, Guid.NewGuid());
        var traveler = BatchTraveler.Create(
            batch.Id, 1, "John", "Doe", null, null,
            "A12345678", "US", new DateTime(1990, 1, 15),
            TravelerGender.Male, DateTime.UtcNow.AddDays(30), "CAI", null,
            DateTime.UtcNow.AddYears(5), "United States", "Tourism", null,
            "uploads/batch-traveler-documents/existing-passport.pdf",
            "uploads/batch-traveler-documents/existing-ticket.pdf");
        batch.AddTraveler(traveler);
        travelerId = traveler.Id;
        return batch;
    }

    private static UpdateTravelerInBatchCommand UpdateCommand(Guid batchId, Guid travelerId) => new(
        BatchId: batchId,
        TravelerId: travelerId,
        FirstNameEn: "Jane",
        LastNameEn: "Smith",
        FirstNameAr: "جين",
        LastNameAr: "سميث",
        PassportNumber: "B98765432",
        NationalityCode: "GB",
        DateOfBirth: new DateTime(1985, 6, 20),
        Gender: TravelerGender.Female,
        TravelDate: DateTime.UtcNow.AddDays(60),
        ArrivalAirport: "HRG",
        TransitCountries: null,
        PassportExpiry: DateTime.UtcNow.AddYears(5),
        DepartureCountry: "United Kingdom",
        PurposeOfTravel: "Business",
        FlightNumber: null,
        PassportImageDocumentPath: null,
        TicketImageDocumentPath: null);

    [Fact]
    public async Task Handle_DraftBatch_UpdatesTravelerSuccessfully()
    {
        // Arrange
        var batch = CreateBatchWithTraveler(out var travelerId);
        _batchRepository.GetByIdWithTravelersAsync(batch.Id, Arg.Any<CancellationToken>()).Returns(batch);

        // Act
        var result = await _sut.Handle(UpdateCommand(batch.Id, travelerId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedTraveler = batch.Travelers.First(t => t.Id == travelerId);
        updatedTraveler.FirstNameEn.Should().Be("Jane");
        updatedTraveler.LastNameEn.Should().Be("Smith");
        updatedTraveler.PassportNumber.Should().Be("B98765432");
        updatedTraveler.PassportImageDocumentPath.Should().Be("uploads/batch-traveler-documents/existing-passport.pdf");
        updatedTraveler.TicketImageDocumentPath.Should().Be("uploads/batch-traveler-documents/existing-ticket.pdf");
        await _batchRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReplacementDocumentPaths_UpdatesStoredDocuments()
    {
        // Arrange
        var batch = CreateBatchWithTraveler(out var travelerId);
        _batchRepository.GetByIdWithTravelersAsync(batch.Id, Arg.Any<CancellationToken>()).Returns(batch);
        var command = UpdateCommand(batch.Id, travelerId) with
        {
            PassportImageDocumentPath = "uploads/batch-traveler-documents/new-passport.pdf",
            TicketImageDocumentPath = "uploads/batch-traveler-documents/new-ticket.pdf"
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedTraveler = batch.Travelers.First(t => t.Id == travelerId);
        updatedTraveler.PassportImageDocumentPath.Should().Be("uploads/batch-traveler-documents/new-passport.pdf");
        updatedTraveler.TicketImageDocumentPath.Should().Be("uploads/batch-traveler-documents/new-ticket.pdf");
    }

    [Fact]
    public async Task Handle_BatchNotFound_ReturnsFailure()
    {
        // Arrange
        _batchRepository.GetByIdWithTravelersAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Batch?)null);

        // Act
        var result = await _sut.Handle(UpdateCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Batch not found.");
    }

    [Fact]
    public async Task Handle_NonDraftBatch_ReturnsFailure()
    {
        // Arrange
        var batch = CreateBatchWithTraveler(out var travelerId);
        batch.SubmitForReview();
        _batchRepository.GetByIdWithTravelersAsync(batch.Id, Arg.Any<CancellationToken>()).Returns(batch);

        // Act
        var result = await _sut.Handle(UpdateCommand(batch.Id, travelerId), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Travelers can only be updated in batches with Draft status.");
    }

    [Fact]
    public async Task Handle_TravelerNotFound_ReturnsFailure()
    {
        // Arrange
        var batch = CreateBatchWithTraveler(out _);
        _batchRepository.GetByIdWithTravelersAsync(batch.Id, Arg.Any<CancellationToken>()).Returns(batch);

        // Act
        var result = await _sut.Handle(UpdateCommand(batch.Id, Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Traveler not found in this batch.");
    }
}
