using CashCanvas.Data.Repository;
using CashCanvas.Dtos;
using CashCanvas.Entities;
using CashCanvas.Services;
using Moq;
using System.Linq.Expressions;

namespace CashCanvas.Tests.Unit.Services;

public class UserStatsServiceUnitTests
{
    private readonly Mock<IRepository<UserStats>> _mockStatsRepository;
    private readonly UserStatsService _sut; // System Under Test

    public UserStatsServiceUnitTests()
    {
        _mockStatsRepository = new Mock<IRepository<UserStats>>();
        _sut = new UserStatsService(_mockStatsRepository.Object);
    }

    [Fact]
    public async Task GetStatsAsync_ShouldReturnExistingStats_WhenStatsExist()
    {
        // Arrange
        var userId = "test-user";
        var existingStats = new UserStats { UserId = userId, TotalCounterClicks = 10 };
        
        _mockStatsRepository
            .Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<UserStats, bool>>>()))
            .ReturnsAsync(existingStats);

        // Act
        var result = await _sut.GetStatsAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(10, result.TotalCounterClicks);
        _mockStatsRepository.Verify(repo => repo.AddAsync(It.IsAny<UserStats>()), Times.Never);
        _mockStatsRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task GetStatsAsync_ShouldCreateAndReturnNewStats_WhenStatsDoNotExist()
    {
        // Arrange
        var userId = "new-user";
        
        _mockStatsRepository
            .Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<UserStats, bool>>>()))
            .ReturnsAsync((UserStats?)null);

        // Act
        var result = await _sut.GetStatsAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(0, result.TotalCounterClicks);
        _mockStatsRepository.Verify(repo => repo.AddAsync(It.Is<UserStats>(s => s.UserId == userId && s.TotalCounterClicks == 0)), Times.Once);
        _mockStatsRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateStatsAsync_ShouldUpdateExistingStats_WhenStatsExist()
    {
        // Arrange
        var userId = "test-user";
        var statsDto = new UserStatsDto { UserId = userId, TotalCounterClicks = 15 };
        var existingStats = new UserStats { UserId = userId, TotalCounterClicks = 10 };

        _mockStatsRepository
            .Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<UserStats, bool>>>()))
            .ReturnsAsync(existingStats);

        // Act
        await _sut.UpdateStatsAsync(statsDto);

        // Assert
        _mockStatsRepository.Verify(repo => repo.Update(It.Is<UserStats>(s => s.TotalCounterClicks == 15)), Times.Once);
        _mockStatsRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        Assert.Equal(15, existingStats.TotalCounterClicks); // Verify the entity was modified
    }

    [Fact]
    public async Task UpdateStatsAsync_ShouldNotCallUpdate_WhenStatsDoNotExist()
    {
        // Arrange
        var statsDto = new UserStatsDto { UserId = "non-existent-user", TotalCounterClicks = 5 };

        _mockStatsRepository
            .Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<UserStats, bool>>>()))
            .ReturnsAsync((UserStats?)null);

        // Act
        await _sut.UpdateStatsAsync(statsDto);

        // Assert
        _mockStatsRepository.Verify(repo => repo.Update(It.IsAny<UserStats>()), Times.Never);
        _mockStatsRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }
}
