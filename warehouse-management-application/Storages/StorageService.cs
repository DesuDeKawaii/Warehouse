using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using warehouse_management_core.DTO_s;
using warehouse_management_core.Entities;

namespace warehouse_management_application.Storages
{
    public class StorageService(IRepository<Storage> repository) : IServise
    {
        private IRepository<Storage> Repository { get; init; } = repository;

        public async Task<IEnumerable<StorageDTO>> GetStoragesAsync(CancellationToken cancellationToken = default) =>
            (await Repository.Get(cancellationToken)).Select(x => (StorageDTO)x);

        public async Task<IEnumerable<StorageDTO>> GetStoragesItemsAsync(Guid storageId, CancellationToken cancellationToken = default)
        {
            var potentialStorage = (await Repository.GetWithoutTracking(x => x.Id.Value == storageId, cancellationToken)).FirstOrDefault() ??
                throw new StorageNotFoundException(storageId);
            return (IEnumerable<StorageDTO>)potentialStorage.ItemsStorage.Cast<ItemDTO>();
        }

        public async Task<IEnumerable<EmployeeDTO>> GetStorageEmployeesAsync(Guid storageId, CancellationToken cancellationToken = default)
        {
            var potentialStorage = (await Repository.GetWithoutTracking(x => x.Id.Value == storageId, cancellationToken)).FirstOrDefault() ??
                throw new StorageNotFoundException(storageId);
            return potentialStorage.Employees.Cast<EmployeeDTO>();
        }

        public async Task CreateOrUpdateStorageAsync(StorageDTO storage, CancellationToken cancellationToken = default)
        {
            Storage localStorage;
            if (storage.Id is not null)
            {
                localStorage = (await Repository.Get(x => x.Id.Value == storage.Id.Value, cancellationToken)).FirstOrDefault() ??
                    throw new StorageNotFoundException(storage.Id.Value);
                localStorage.Name = storage.Name;
                localStorage.Description = storage.Description;
                localStorage.Capacity = storage.Capacity;
                localStorage.Temperature = storage.Temperature;

            }
            else
                localStorage = new()
                {
                    Name = storage.Name,
                    Description = storage.Description,
                    Capacity = storage.Capacity,
                    Temperature = storage.Temperature
                };

            if (localStorage.Id is null)
                await Repository.Add(localStorage, cancellationToken);
            else
                await Repository.Update(localStorage, cancellationToken);
        }

        public async Task GetStorageItems(Guid storageId, Guid itemId, ItemStorage itemStorage, CancellationToken cancellationToken = default)
        {
            var potentialStorage = (await Repository.GetWithoutTracking(x => x.Id.Value == storageId, cancellationToken)).FirstOrDefault() ??
                  throw new StorageNotFoundException(storageId);
            var item = potentialStorage.ItemsStorage.FirstOrDefault(x => x.Id.Value == itemId) ??
                throw new ItemNotFoundException(itemId);

            if ((item.Amount - itemStorage.Amount) >= 0)
                item.Amount -= itemStorage.Amount;
            else
                throw new ItemOutOfStockException(itemId, itemStorage.Amount);

            if (item.Amount == 0)
                potentialStorage.ItemsStorage.Remove(item);

            await Repository.Update(potentialStorage, cancellationToken);
        }

        public async Task BuyItems(Guid storageId, ItemStorage itemStorage, CancellationToken cancellationToken = default)
        {
            var potentialStorage = (await Repository.Get(x => x.Id.Value == storageId, cancellationToken)).FirstOrDefault() ??
                throw new StorageNotFoundException(storageId);

            var item = potentialStorage.ItemsStorage.FirstOrDefault(x => x.Item == itemStorage.Item);
            if (item is null)
                potentialStorage.ItemsStorage.Add(new()
                {
                    Item = itemStorage.Item,
                    Amount = itemStorage.Amount
                });
            else
                item.Amount += itemStorage.Amount;

            await Repository.Update(potentialStorage, cancellationToken);
        }


    }
}
