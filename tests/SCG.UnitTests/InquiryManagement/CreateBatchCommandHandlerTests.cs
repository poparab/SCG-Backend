using SCG.InquiryManagement.Application.Abstractions;
using SCG.InquiryManagement.Application.Commands.CreateBatch;
using SCG.InquiryManagement.Domain.Entities;

namespace SCG.UnitTests.InquiryManagement;

public sealed class CreateBatchCommandHandlerTests
{
    private readonly IBatchRepository _batchRepository = Substitute.For<IBatchRepository>();
    private readonly CreateBatchCommandHandler _sut;

    public CreateBatchCommandHandlerTests()
    {
        _sut = new CreateBatchCommandHandler(_batchRepository);
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesBatchInDraftStatus()
    {
        // Arrange
        var command = new CreateBatchCommand(
            Guid.NewGuid(), Guid.NewGuid(), "Test Batch", Guid.NewGuid(), null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _batchRepository.Received(1).AddAsync(
            Arg.Is<Batch>(b => b.Name == "Test Batch"),
            Arg.Any<CancellationToken>());
        await _batchRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmptyName_ReturnsFailure()
    {
        // Arrange
        var command = new CreateBatchCommand(
            Guid.NewGuid(), Guid.NewGuid(), "", Guid.NewGuid(), null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Batch name is required.");
    }

    [Fact]
    public async Task Handle_WhitespaceName_ReturnsFailure()
    {
        // Arrange
        var command = new CreateBatchCommand(
            Guid.NewGuid(), Guid.NewGuid(), "   ", Guid.NewGuid(), null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Batch name is required.");
    }

    [Fact]
    public async Task Handle_WithNotes_CreatesBatchWithNotes()
    {
        // Arrange
        var command = new CreateBatchCommand(
            Guid.NewGuid(), Guid.NewGuid(), "Batch", Guid.NewGuid(), "Some notes");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _batchRepository.Received(1).AddAsync(
            Arg.Is<Batch>(b => b.Notes == "Some notes"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TrimsName()
    {
        // Arrange
        var command = new CreateBatchCommand(
            Guid.NewGuid(), Guid.NewGuid(), "  Batch Name  ", Guid.NewGuid(), null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _batchRepository.Received(1).AddAsync(
            Arg.Is<Batch>(b => b.Name == "Batch Name"),
            Arg.Any<CancellationToken>());
    }
}
