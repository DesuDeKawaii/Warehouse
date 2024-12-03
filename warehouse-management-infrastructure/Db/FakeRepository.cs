using Microsoft.EntityFrameworkCore;
using warehouse_management_core;
using warehouse_management_core.Entities;

namespace warehouse_management_infrastructure.Db;

public class FakeRepository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
{
    private readonly FakeContext _context;

    // Конструктор для инъекции зависимости контекста
    public FakeRepository(FakeContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));  // Проверка на null
        if (!_context.Set<Item>().Any())
        {
            Item[] items = [
                new Item {
                        Description = "Test",
                        Name = "Test",
                        Price = 1,
                        ExpirationDate = DateTime.Now,
                        Id = new Id(Guid.NewGuid()),
                    },
            new Item {
                        Description = "Test2",
                        Name = "Test2",
                        Price = 2,
                        ExpirationDate = DateTime.Now,
                        Id = new Id(Guid.NewGuid()),
                     }
            ];
            Employee[] employees = [

                new Employee
                {
                    Id = new Id(Guid.NewGuid()),
                    Name = "Test",
                    Email = "Test",
                    Phone = "11111",
                    Salary = 100
                },
                new Employee
                {
                    Id = new Id(Guid.NewGuid()),
                    Name = "Test2",
                    Email = "Test2",
                    Phone = "22222",
                    Salary = 200
                }
            ];
            Storage[] storages = [

                new Storage
                {
                    Id = new Id(Guid.NewGuid()),
                    Name = "Test",
                    Capacity = 10,
                    Description = "Test",
                    Temperature = 1,
                    Latitude = 1,
                    Longitude = 1,
                    Employees = [employees[0]]

                }
            ];
            Shop[] shops = [
                new Shop
                {
                    Id = new Id(Guid.NewGuid()),
                    Name = "Test",
                    Capacity = 10,
                    Latitude = 1,
                    Longitude = 1,
                    Employees =[employees[1]]
                }

            ];
            ItemShop[] itemShops = [
                new ItemShop
                {
                    Id = new Id(Guid.NewGuid()),
                    Item = items[0],
                    Amount = 2,
                    Shop = shops[0]
                },

                new ItemShop
                {
                    Id = new Id(Guid.NewGuid()),
                    Item = items[1],
                    Amount = 2,
                    Shop = shops[0]
                }
            ];
            ItemStorage[] itemStorages = [
                new ItemStorage
                {
                    Id = new Id(Guid.NewGuid()),
                    Item = items[0],
                    Amount = 2,
                    Storage = storages[0]
                },

                new ItemStorage
                {
                    Id = new Id(Guid.NewGuid()),
                    Item = items[1],
                    Amount = 2,
                    Storage = storages[0]
                }
            ];

            _context.AddRange([.. items, .. employees, .. storages, .. shops, .. itemShops, .. itemStorages]);
            _context.SaveChanges();
        }
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