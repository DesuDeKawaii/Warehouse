using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using warehouse_management_core;

namespace warehouse_management_infrastructure.Db
{
    public class BasicRepository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
    {
        private readonly WarehouseContext _context;

        // Конструктор для инъекции зависимости контекста
        public BasicRepository(WarehouseContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));  // Проверка на null
        }

        public async Task AddRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
        {
            if (_context == null) throw new InvalidOperationException("DbContext is not initialized.");

            await _context.AddRangeAsync(entities, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public Task<IEnumerable<TEntity>> Get(CancellationToken cancellationToken)
        {
            if (_context == null) throw new InvalidOperationException("DbContext is not initialized.");

            return Task.FromResult(_context.Set<TEntity>().ToArray().AsEnumerable());
        }

        public Task<IEnumerable<TEntity>> Get(Func<TEntity, bool> predicate, CancellationToken cancellationToken)
        {
            if (_context == null) throw new InvalidOperationException("DbContext is not initialized.");

            return Task.FromResult(_context.Set<TEntity>().Where(predicate).ToArray().AsEnumerable());
        }

        public Task Get(Id id, CancellationToken cancellationToken)
        {
            if (_context == null) throw new InvalidOperationException("DbContext is not initialized.");

            return Task.FromResult(_context.Set<TEntity>().ToArray().AsEnumerable());
        }

        public Task<IEnumerable<TEntity>> GetWithoutTracking(CancellationToken cancellationToken)
        {
            if (_context == null) throw new InvalidOperationException("DbContext is not initialized.");

            return Task.FromResult(_context.Set<TEntity>().AsNoTracking().ToArray().AsEnumerable());
        }

        public Task<IEnumerable<TEntity>> GetWithoutTracking(Func<TEntity, bool> predicate, CancellationToken cancellationToken)
        {
            if (_context == null) throw new InvalidOperationException("DbContext is not initialized.");

            return Task.FromResult(_context.Set<TEntity>().AsNoTracking().Where(predicate).ToArray().AsEnumerable());
        }

        public Task RemoveRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
        {
            if (_context == null) throw new InvalidOperationException("DbContext is not initialized.");

            return Task.Run(() =>
            {
                _context.RemoveRange(entities);
                _context.SaveChanges();
            }, cancellationToken);
        }

        public Task UpdateRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
        {
            if (_context == null) throw new InvalidOperationException("DbContext is not initialized.");

            return Task.Run(() =>
            {
                _context.UpdateRange(entities);
                _context.SaveChanges();
            }, cancellationToken);
        }
    }
}
