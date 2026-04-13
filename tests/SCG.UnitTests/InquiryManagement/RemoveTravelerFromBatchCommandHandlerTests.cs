using SCG.InquiryManagement.Application.Abstractions;
using SCG.InquiryManagement.Application.Commands.RemoveTravelerFromBatch;
using SCG.InquiryManagement.Domain.Entities;
using SCG.InquiryManagement.Domain.Enums;

namespace SCG.UnitTests.InquiryManagement;

public sealed class RemoveTravelerFromBatchCommandHandlerTests
{
    private readonly IBatchRepository _batchRepository = Substitute.For<IBatchRepository>();
    private readonly RemoveTravelerFromBatchCommandHandler _sut;

    public RemoveTravelerFromBatchCommandHandlerTests()
    {
        _sut = new RemoveTravelerFromBatchCommandHandler(_batchRepository);
    }

    private static Batch CreateBatchWithTraveler(out Guid travelerId)
    {
        var batch = Batch.Create(Guid.NewGuid(), "Test Batch", Guid.NewGuid(), null, Guid.NewGuid());
        var traveler = BatchTraveler.Create(
            batch.Id, 1, "John", "Doe", null, null,
            "A12345678", "US", new DateTime(1990, 1, 15),
            TravelerGender.Male, DateTime.UtcNow.AddDays(30), "CAI", null,
            DateTime.UtcNow.AddYears(5), "United States", "Tourism", null,
            "uploads/batch-traveler-documents/passport.pdf",
            "uploads/batch-traveler-documents/ticket.pdf");
        batch.AddTraveler(traveler);
        travelerId = traveler.Id;
        return batch;
    }

    [Fact]
    public async Task Handle_DraftBatch_RemovesTravelerSuccessfully()
    {
        // Arrange
        var batch = CreateBatchWithTraveler(out var travelerId);
        _batchRepository.GetByIdWithTravelersAsync(batch.Id, Arg.Any<CancellationToken>()).Returns(batch);

        // Act
        var result = await _sut.Handle(
            new RemoveTravelerFromBatchCommand(batch.Id, travelerId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        batch.TravelerCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_BatchNotFound_ReturnsFailure()
    {
        // Arrange
        _batchRepository.GetByIdWithTravelersAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Batch?)null);

        // Act
        var result = await _sut.Handle(
            new RemoveTravelerFromBatchCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

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
        var result = await _sut.Handle(
            new RemoveTravelerFromBatchCommand(batch.Id, travelerId), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Travelers can only be removed from batches in Draft status.");
    }

    [Fact]
    public async Task Handle_TravelerNotFound_ReturnsFailure()
    {
        // Arrange
        var batch = CreateBatchWithTraveler(out _);
        _batchRepository.GetByIdWithTravelersAsync(batch.Id, Arg.Any<CancellationToken>()).Returns(batch);

        // Act
        var result = await _sut.Handle(
            new RemoveTravelerFromBatchCommand(batch.Id, Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Traveler not found in this batch.");
    }
}
