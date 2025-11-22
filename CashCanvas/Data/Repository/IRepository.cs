namespace CashCanvas.Data.Repository;

// Data/IRepository.cs
using System.Linq.Expressions;

/// <summary>
/// Represents a generic repository for data access operations.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Gets an entity by its primary key asynchronously.
    /// </summary>
    /// <param name="id">The primary key of the entity to find.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the entity found, or null.</returns>
    Task<TEntity?> GetByIdAsync(object id);

    /// <summary>
    /// Gets all entities of the specified type asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of all entities.</returns>
    Task<IEnumerable<TEntity>> GetAllAsync();

    /// <summary>
    /// Finds the first entity that matches the predicate, or a default value if no entity is found, asynchronously.
    /// </summary>
    /// <param name="predicate">The condition to test each element for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the first entity that matches the predicate, or null if none is found.</returns>
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Adds a new entity to the data store asynchronously.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddAsync(TEntity entity);

    /// <summary>
    /// Marks an existing entity as modified.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    void Update(TEntity entity);

    /// <summary>
    /// Marks an existing entity for deletion.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    void Delete(TEntity entity);

    /// <summary>
    /// Saves all changes made in this context to the underlying database asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync();
}
