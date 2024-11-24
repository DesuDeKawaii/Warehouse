using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using warehouse_management_core;
using warehouse_management_core.DTO_s;
using warehouse_management_core.Entities;

namespace warehouse_management_application.Storages
{
    public class StorageService(IRepository<Storage> repository, IMapProvider mapProvider) : IServise
    {
        private IRepository<Storage> Repository { get; init; } = repository;
        private IMapProvider MapProvider { get; init; } = mapProvider;

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


        private async Task<Storage> FindSuitableStorageAsync(Item item, Storage currentStorage, CancellationToken cancellationToken)
        {
            var suitableStorages = await Repository.GetWithoutTracking(x =>
                x.Id != currentStorage.Id &&
                x.Capacity > x.ItemsStorage.Sum(x => x.Amount) &&
                x.Temperature <= item.Temperature, cancellationToken);

            var currentStorageCoordinates = await MapProvider.GetCoordinatesAsync(currentStorage.Name, cancellationToken);

            var sortedStorages = suitableStorages
                .Select(async x =>
                {
                    var coordinates = await MapProvider.GetCoordinatesAsync(x.Name, cancellationToken);
                    var distance = MapProvider.CalculateDistance(currentStorageCoordinates.Latitude, currentStorageCoordinates.Longitude, coordinates.Latitude, coordinates.Longitude);
                    return new { Storage = x, Distance = distance };
                }).Select(x => x.Result)
                .OrderBy(x => x.Distance)
                .Select(x => x.Storage)
                .ToList();

            return sortedStorages.FirstOrDefault();
        }


        public async Task MoveItemsIfOverflowAsync(Guid storageId, CancellationToken cancellationToken = default)
        {
            var storage = (await Repository.GetWithoutTracking(x => x.Id.Value == storageId, cancellationToken)).FirstOrDefault() ??
                          throw new StorageNotFoundException(storageId);

            if (storage.ItemsStorage.Sum(x => x.Amount) > storage.Capacity)
            {
                var itemsToMove = storage.ItemsStorage.ToList();

                foreach (var itemStorage in itemsToMove)
                {
                    var suitableStorage = await FindSuitableStorageAsync(itemStorage.Item, storage, cancellationToken);

                    if (suitableStorage != null)
                    {
                        await TransferItemAsync(itemStorage, suitableStorage, cancellationToken);
                    }
                }
            }
        }
        private async Task TransferItemAsync(ItemStorage itemStorage, Storage targetStorage, CancellationToken cancellationToken)
        {
            var itemInTargetStorage = targetStorage.ItemsStorage.FirstOrDefault(x => x.Item.Id == itemStorage.Item.Id);
            if (itemInTargetStorage != null)
            {
                itemInTargetStorage.Amount += itemStorage.Amount;
            }
            else
            {
                targetStorage.ItemsStorage.Add(new ItemStorage
                {
                    Item = itemStorage.Item,
                    Amount = itemStorage.Amount,
                    Storage = targetStorage
                });
            }

            var currentStorage = itemStorage.Storage;
            currentStorage.ItemsStorage.Remove(itemStorage);

            await Repository.Update(currentStorage, cancellationToken);
            await Repository.Update(targetStorage, cancellationToken);
        }

        public async Task<Storage> GetStorageById(Guid storageId, CancellationToken cancellationToken)
        {
            var storage = (await Repository.Get(x => x.Id.Value == storageId, cancellationToken)).FirstOrDefault() ??
                throw new StorageNotFoundException(storageId);
            return storage;
        }

        public async Task DeleteStorageAsync(Guid storageId, CancellationToken cancellationToken = default)
        {
            var potentialStorage = (await Repository.Get(x => x.Id.Value == storageId, cancellationToken)).FirstOrDefault() ??
                throw new StorageNotFoundException(storageId);
            await Repository.Remove(potentialStorage, cancellationToken);
        }
    }
}