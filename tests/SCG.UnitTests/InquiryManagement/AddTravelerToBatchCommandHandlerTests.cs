using SCG.InquiryManagement.Application.Abstractions;
using SCG.InquiryManagement.Application.Commands.AddTravelerToBatch;
using SCG.InquiryManagement.Domain.Entities;
using SCG.InquiryManagement.Domain.Enums;

namespace SCG.UnitTests.InquiryManagement;

public sealed class AddTravelerToBatchCommandHandlerTests
{
    private readonly IBatchRepository _batchRepository = Substitute.For<IBatchRepository>();
    private readonly AddTravelerToBatchCommandHandler _sut;

    public AddTravelerToBatchCommandHandlerTests()
    {
        _sut = new AddTravelerToBatchCommandHandler(_batchRepository);
    }

    private static AddTravelerToBatchCommand ValidCommand(Guid batchId) => new(
        BatchId: batchId,
        FirstNameEn: "John",
        LastNameEn: "Doe",
        FirstNameAr: "جون",
        LastNameAr: "دو",
        PassportNumber: "A12345678",
        NationalityCode: "US",
        DateOfBirth: new DateTime(1990, 1, 15),
        Gender: TravelerGender.Male,
        TravelDate: DateTime.UtcNow.AddDays(30),
        ArrivalAirport: "CAI",
        TransitCountries: null,
        PassportExpiry: DateTime.UtcNow.AddYears(5),
        DepartureCountry: "United States",
        PurposeOfTravel: "Tourism",
        FlightNumber: null,
        PassportImageDocumentPath: "uploads/batch-traveler-documents/passport.pdf",
        TicketImageDocumentPath: "uploads/batch-traveler-documents/ticket.pdf");

    private static Batch CreateDraftBatch()
    {
        return Batch.Create(Guid.NewGuid(), "Test Batch", Guid.NewGuid(), null, Guid.NewGuid());
    }

    [Fact]
    public async Task Handle_DraftBatch_AddsTravelerSuccessfully()
    {
        // Arrange
        var batch = CreateDraftBatch();
        _batchRepository.GetByIdWithTravelersAsync(batch.Id, Arg.Any<CancellationToken>()).Returns(batch);

        // Act
        var result = await _sut.Handle(ValidCommand(batch.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        batch.TravelerCount.Should().Be(1);
        batch.Travelers[0].PassportImageDocumentPath.Should().Be("uploads/batch-traveler-documents/passport.pdf");
        batch.Travelers[0].TicketImageDocumentPath.Should().Be("uploads/batch-traveler-documents/ticket.pdf");
        await _batchRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_BatchNotFound_ReturnsFailure()
    {
        // Arrange
        _batchRepository.GetByIdWithTravelersAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Batch?)null);

        // Act
        var result = await _sut.Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Batch not found.");
    }

    [Fact]
    public async Task Handle_NonDraftBatch_ReturnsFailure()
    {
        // Arrange
        var batch = CreateDraftBatch();
        batch.SubmitForReview(); // Status is now PendingReview
        _batchRepository.GetByIdWithTravelersAsync(batch.Id, Arg.Any<CancellationToken>()).Returns(batch);

        // Act
        var result = await _sut.Handle(ValidCommand(batch.Id), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Travelers can only be added to batches in Draft status.");
    }

    [Fact]
    public async Task Handle_MultipleTravelers_IncrementsRowIndex()
    {
        // Arrange
        var batch = CreateDraftBatch();
        _batchRepository.GetByIdWithTravelersAsync(batch.Id, Arg.Any<CancellationToken>()).Returns(batch);

        // Act — add two travelers
        await _sut.Handle(ValidCommand(batch.Id), CancellationToken.None);
        var result = await _sut.Handle(ValidCommand(batch.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        batch.TravelerCount.Should().Be(2);
    }
}
