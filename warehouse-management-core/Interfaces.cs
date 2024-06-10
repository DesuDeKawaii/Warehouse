namespace warehouse_management_core;

public record Id(Guid Value);
public interface IEntity
{
    Id Id { get; }
}

public interface IRepository<TEntity> where TEntity : class, IEntity
{
    Task AddRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken);
    Task Add(TEntity entity, CancellationToken cancellationToken) =>
        AddRange([entity], cancellationToken);

    Task UpdateRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken);
    Task Update(TEntity entity, CancellationToken cancellationToken) =>
        UpdateRange([entity], cancellationToken);
    Task RemoveRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken);
    Task Remove(TEntity entity, CancellationToken cancellationToken) =>
        RemoveRange([entity], cancellationToken);

    Task<IEnumerable<TEntity>> Get(CancellationToken cancellationToken);
    Task Get(Id id, CancellationToken cancellationToken) =>
        Get(id, cancellationToken);
    Task<IEnumerable<TEntity>> Get(Func<TEntity, bool> predicate, CancellationToken cancellationToken);
    Task<IEnumerable<TEntity>> GetWithoutTracking(CancellationToken cancellationToken);
    Task<IEnumerable<TEntity>> GetWithoutTracking(Func<TEntity, bool> predicate, CancellationToken cancellationToken);
}
public interface IMapProvider
{
    Task<(double Latitude, double Longitude)> GetCoordinatesAsync(string address, CancellationToken cancellationToken = default);
    double CalculateDistance(double latitude1, double longitude1, double latitude2, double longitude2);
}