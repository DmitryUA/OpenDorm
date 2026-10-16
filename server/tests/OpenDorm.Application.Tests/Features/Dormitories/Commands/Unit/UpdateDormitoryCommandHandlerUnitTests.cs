using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Features.Dormitories.Commands.UpdateDormitory;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Application.Tests.Features.Dormitories.Commands.Unit;

public class UpdateDormitoryCommandHandlerUnitTests
{
    private readonly IDormitoryRepository _mockRepository;
    private readonly UpdateDormitoryCommandHandler _handler;

    public UpdateDormitoryCommandHandlerUnitTests()
    {
        _mockRepository = Substitute.For<IDormitoryRepository>();
        var uow = Substitute.For<IUnitOfWork>();
        _handler = new UpdateDormitoryCommandHandler(_mockRepository, uow);
    }

    [Fact]
    public async Task Handle_NonExistingDormitory_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistingDormitoryId = Guid.NewGuid();
        var command = new UpdateDormitoryCommand(
            nonExistingDormitoryId,
            City: "Москва",
            Street: "Тверская",
            House: "1");

        _mockRepository
            .GetByIdAsync(nonExistingDormitoryId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Dormitory?>(null));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal(nameof(Dormitory), exception.EntityName);
        Assert.Equal(nonExistingDormitoryId, exception.EntityId);
    }
}