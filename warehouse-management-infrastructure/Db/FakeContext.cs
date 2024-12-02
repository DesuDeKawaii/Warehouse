using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore;
using warehouse_management_core.Entities;
using warehouse_management_core;

namespace warehouse_management_infrastructure.Db
{

    public class FakeContext : DbContext
    {
        public class IdConverter : ValueConverter<Id, Guid>
        {
            public IdConverter()
                : base(
                    id => id.Value,  // Преобразуем Id в Guid
                    guid => new Id(guid)) // Преобразуем Guid в Id
            { }
        }


        public DbSet<Item> Items { get; set; } = null!;
        public DbSet<ItemShop> ItemShops { get; set; } = null!;
        public DbSet<ItemStorage> ItemStorages { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<Shop> Shops { get; set; } = null!;
        public DbSet<Storage> Storages { get; set; } = null!;



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Применяем конвертер для всех свойств типа Id
            modelBuilder.Entity<Item>()
                .Property(i => i.Id)
                .HasConversion(new IdConverter());  // Преобразуем Id в Guid для Item

            modelBuilder.Entity<ItemShop>()
                .Property(i => i.Id)
                .HasConversion(new IdConverter());  // Преобразуем Id в Guid для ItemShop

            modelBuilder.Entity<ItemStorage>()
                .Property(i => i.Id)
                .HasConversion(new IdConverter());  // Преобразуем Id в Guid для ItemStorage

            // Явно указываем внешний ключ для связи один к одному между Item и ItemShop
            modelBuilder.Entity<Item>()
                .HasOne(i => i.ItemShop)  // Связь с ItemShop
                .WithOne(i => i.Item)        // Связь с Item
                .HasForeignKey<ItemShop>(i => i.Id)  // Указываем внешний ключ для ItemShop
                .OnDelete(DeleteBehavior.Restrict); // Указываем поведение при удалении, если нужно

            // Явно указываем внешний ключ для связи один к одному между Item и ItemStorage
            modelBuilder.Entity<Item>()
                .HasOne(i => i.ItemStorage)  // Связь с ItemStorage
                .WithOne(i => i.Item)        // Связь с Item
                .HasForeignKey<ItemStorage>(i => i.Id)  // Указываем внешний ключ для ItemStorage
                .OnDelete(DeleteBehavior.Restrict); // Указываем поведение при удалении, если нужно

            modelBuilder.Entity<Employee>()
          .HasKey(e => e.Id);  // Указываем, что Id является первичным ключом

            // Применяем конвертер для типа Id
            modelBuilder.Entity<Employee>()
                .Property(e => e.Id)
                .HasConversion(new IdConverter());

            modelBuilder.Entity<Shop>()
          .HasKey(e => e.Id);  // Указываем, что Id является первичным ключом

            // Применяем конвертер для типа Id
            modelBuilder.Entity<Shop>()
                .Property(e => e.Id)
                .HasConversion(new IdConverter());

            modelBuilder.Entity<Storage>()
          .HasKey(e => e.Id);  // Указываем, что Id является первичным ключом

            // Применяем конвертер для типа Id
            modelBuilder.Entity<Storage>()
                .Property(e => e.Id)
                .HasConversion(new IdConverter());

            // Применяем дополнительные конфигурации из сборки
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(WarehouseContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }





        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies()
                .UseInMemoryDatabase("FakeDatabase");
            base.OnConfiguring(optionsBuilder);
        }
    }


}
