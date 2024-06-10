using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using warehouse_management_core;

namespace warehouse_management_infrastructure.Db;

internal class WarehouseContext : DbContext
{
    class IdConverter() : ValueConverter<Guid, Id>(x => new Id(x), x => x.Value);
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entityTypes = typeof(IEntity).Assembly
                                        .GetTypes()
                                        .Where(x => !x.IsInterface &&
                                                    !x.IsAbstract && 
                                                    typeof(IEntity).IsAssignableFrom(x));
        foreach (var entityType in entityTypes)
        {
            modelBuilder.Entity(entityType)
                .Property(nameof(IEntity.Id))
                .HasConversion<IdConverter>()
                .HasDefaultValueSql("NEWSEQUENTIALID()");
        }
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WarehouseContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies()
                      .UseSqlServer("Server=db;Database=warehousedb;User Id=sa;Password=12345");
        base.OnConfiguring(optionsBuilder);
    }
}
