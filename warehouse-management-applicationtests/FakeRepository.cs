using warehouse_management_core;

namespace warehouse_management_applicationtests
{
    internal class FakeRepository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
    {
        private List<TEntity> _entities = [];

        public Task AddRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) =>
            Task.Run(() =>
            {
                _entities.AddRange(entities);
            }, cancellationToken);

        public Task<IEnumerable<TEntity>> Get(CancellationToken cancellationToken = default) =>
            Task.FromResult(_entities.AsEnumerable());

        public Task<IEnumerable<TEntity>> Get(Func<TEntity, bool> predicate, CancellationToken cancellationToken = default) =>
            Task.FromResult(_entities.Where(predicate).AsEnumerable());

        public Task<IEnumerable<TEntity>> GetWithoutTracking(CancellationToken cancellationToken = default) =>
            Get(cancellationToken);
        public Task<IEnumerable<TEntity>> GetWithoutTracking(Func<TEntity, bool> predicate, CancellationToken cancellationToken = default) =>
            Get(predicate, cancellationToken);

        public Task RemoveRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) =>
            Task.Run(() => _entities.RemoveAll(x => entities.Contains(x)));

        public Task UpdateRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
