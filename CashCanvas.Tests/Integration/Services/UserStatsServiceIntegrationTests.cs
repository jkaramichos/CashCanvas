using CashCanvas.Data;
using CashCanvas.Data.Repository;
using CashCanvas.Entities;
using CashCanvas.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using CashCanvas.Dtos;

namespace CashCanvas.Tests.Integration.Services;

public class UserStatsServiceIntegrationTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

    public UserStatsServiceIntegrationTests()
    {
        // Configure the DbContext to use a new in-memory database for each test run.
        // The database name is a GUID to ensure test isolation.
        _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GetStatsAsync_ShouldCreateAndReturnNewStats_WhenStatsDoNotExist()
    {
        // Arrange
        var userId = "new-integration-user";
        // Use a 'using' block to ensure the context is disposed of properly.
        await using var context = new ApplicationDbContext(_dbContextOptions);
        
        var repository = new Repository<UserStats>(context);
        var sut = new UserStatsService(repository);

        // Act
        var result = await sut.GetStatsAsync(userId);

        // Assert
        // 1. Check the returned object from the service method
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(0, result.TotalCounterClicks);

        // 2. Verify the data was actually saved to the database
        var statsInDb = await context.UserStats.FirstOrDefaultAsync(s => s.UserId == userId);
        Assert.NotNull(statsInDb);
        Assert.Equal(userId, statsInDb.UserId);
    }

    [Fact]
    public async Task GetStatsAsync_ShouldReturnExistingStats_WhenStatsExist()
    {
        // Arrange
        var userId = "existing-integration-user";
        var existingStats = new UserStats { UserId = userId, TotalCounterClicks = 50 };

        // Seed the database with starting data for this test
        await using (var context = new ApplicationDbContext(_dbContextOptions))
        {
            context.UserStats.Add(existingStats);
            await context.SaveChangesAsync();
        }

        // Create a new context and service instance for the actual test
        await using (var context = new ApplicationDbContext(_dbContextOptions))
        {
            var repository = new Repository<UserStats>(context);
            var sut = new UserStatsService(repository);

            // Act
            var result = await sut.GetStatsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(50, result.TotalCounterClicks);
            
            // Verify no new record was created
            Assert.Equal(1, await context.UserStats.CountAsync());
        }
    }
    
    [Fact]
    public async Task UpdateStatsAsync_ShouldUpdateDatabase_WhenStatsExist()
    {
        // Arrange
        var userId = "update-integration-user";
        var initialStats = new UserStats { UserId = userId, TotalCounterClicks = 100 };

        // Seed the database
        await using (var context = new ApplicationDbContext(_dbContextOptions))
        {
            context.UserStats.Add(initialStats);
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = new ApplicationDbContext(_dbContextOptions))
        {
            var repository = new Repository<UserStats>(context);
            var sut = new UserStatsService(repository);
            
            var updateDto = new UserStatsDto { UserId = userId, TotalCounterClicks = 150 };
            await sut.UpdateStatsAsync(updateDto);
        }

        // Assert
        // Use a separate context to ensure the data was persisted and not just tracked in memory
        await using (var context = new ApplicationDbContext(_dbContextOptions))
        {
            var updatedStats = await context.UserStats.FirstOrDefaultAsync(s => s.UserId == userId);
            Assert.NotNull(updatedStats);
            Assert.Equal(150, updatedStats.TotalCounterClicks);
        }
    }
}
