using CashCanvas.Data.Repository;
using CashCanvas.Dtos;
using CashCanvas.Entities;
using CashCanvas.Services.Interfaces;

namespace CashCanvas.Services;

public class UserStatsService : IUserStatsService
{
    private readonly IRepository<UserStats> _statsRepository;

    public UserStatsService(IRepository<UserStats> statsRepository)
    {
        _statsRepository = statsRepository;
    }

    public async Task<UserStatsDto> GetStatsAsync(string userId)
    {
        var stats = await _statsRepository.FirstOrDefaultAsync(s => s.UserId == userId);

        if (stats == null)
        {
            stats = new UserStats
            {
                UserId = userId,
                TotalCounterClicks = 0
            };
            await _statsRepository.AddAsync(stats);
            await _statsRepository.SaveChangesAsync();
        }

        return ConvertToDto(stats);
    }

    public async Task UpdateStatsAsync(UserStatsDto statsDto)
    {
        // 1. Retrieve the existing entity from the database.
        var statsEntity = await _statsRepository.FirstOrDefaultAsync(s => s.UserId == statsDto.UserId);

        if (statsEntity != null)
        {
            // 2. Apply the changes from the DTO to the tracked entity.
            statsEntity.TotalCounterClicks = statsDto.TotalCounterClicks;

            // 3. Tell the repository to update the entity and save.
            _statsRepository.Update(statsEntity);
            await _statsRepository.SaveChangesAsync();
        }
    }
    
    /// <summary>
    /// Helper function for converting UserStats entity to UserStatsDto
    /// </summary>
    /// <param name="stats"></param>
    /// <returns></returns>
    private static UserStatsDto ConvertToDto(UserStats stats)
    {
        return new UserStatsDto
        {
            TotalCounterClicks = stats.TotalCounterClicks,
            UserId = stats.UserId
        };
    }
}
