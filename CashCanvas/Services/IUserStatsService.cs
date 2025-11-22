using CashCanvas.Entities;

namespace CashCanvas.Services;

/// <summary>
/// Defines the contract for a service that manages user statistics.
/// </summary>
public interface IUserStatsService
{
    /// <summary>
    /// Gets the statistics for a given user, creating them if they do not exist.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user's statistics.</returns>
    Task<UserStats> GetStatsAsync(string userId);

    /// <summary>
    /// Updates the statistics for a user.
    /// </summary>
    /// <param name="stats">The user statistics to update.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateStatsAsync(UserStats stats);
}

