using System.Linq.Expressions;
using CashCanvas.Data.Repository;
using CashCanvas.Entities;
using CashCanvas.Services;
using Moq;

namespace CashCanvas.Tests.UnitTests.Services;

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
    public async Task UpdateStatsAsync_ShouldCallUpdateAndSaveChangesAsync()
    {
        // Arrange
        var statsToUpdate = new UserStats { UserId = "test-user", TotalCounterClicks = 5 };

        // Act
        await _sut.UpdateStatsAsync(statsToUpdate);

        // Assert
        _mockStatsRepository.Verify(repo => repo.Update(statsToUpdate), Times.Once);
        _mockStatsRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }
}
