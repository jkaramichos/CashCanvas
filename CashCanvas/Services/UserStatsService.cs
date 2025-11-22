using CashCanvas.Data.Repository;
using CashCanvas.Entities;

namespace CashCanvas.Services;

public class UserStatsService : IUserStatsService
{
    private readonly IRepository<UserStats> _statsRepository;

    public UserStatsService(IRepository<UserStats> statsRepository)
    {
        _statsRepository = statsRepository;
    }

    public async Task<UserStats> GetOrCreateStatsAsync(string userId)
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

        return stats;
    }

    public async Task UpdateStatsAsync(UserStats stats)
    {
        _statsRepository.Update(stats);
        await _statsRepository.SaveChangesAsync();
    }
}
