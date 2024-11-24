using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using warehouse_management_core;
using warehouse_management_core.DTO_s;
using warehouse_management_core.Entities;

namespace warehouse_management_application.Shops;

public class ShopService(IRepository<Shop> repository) : IServise
{
    private IRepository<Shop> Repository { get; init; } = repository;
    private IMapProvider MapProvider { get; init; }
    private IRepository<Storage> StorageRepository { get; init; }

    public async Task<IEnumerable<ShopDTO>> GetShopAsync(CancellationToken cancellationToken = default) =>
        (await Repository.Get(cancellationToken)).Select(x => (ShopDTO)x);

    public async Task CreateOrUpdateShopAsync(ShopDTO shop, CancellationToken cancellationToken = default)
    {
        Shop localShop;
        if (shop.Id is not null)
        {
            localShop = (await Repository.Get(x => x.Id.Value == shop.Id.Value, cancellationToken)).FirstOrDefault() ??
                throw new ShopNotFoundException(shop.Id.Value);
            localShop.Name = shop.Name;
            localShop.Capacity = shop.Capacity;
        }
        else
            localShop = new()
            {
                Name = shop.Name,
                Capacity = shop.Capacity
            };

        if (localShop.Id is null)
            await Repository.Add(localShop, cancellationToken);
        else
            await Repository.Update(localShop, cancellationToken);
    }


    public async Task GetItemFromShop(Guid shopId, Guid itemId, ItemShop itemShop, CancellationToken cancellationToken = default)
    {
        var potentialShop = (await Repository.GetWithoutTracking(x => x.Id.Value == shopId, cancellationToken)).FirstOrDefault() ??
                   throw new ShopNotFoundException(shopId);
        var item = potentialShop.ItemsShop.FirstOrDefault(x => x.Id.Value == itemId) ??
            throw new ItemNotFoundException(itemId);

    }

    public async Task SellShopItems(Guid shopId, Guid itemId, ItemShop itemShop, CancellationToken cancellationToken = default)
    {
        var potentialShop = (await Repository.GetWithoutTracking(x => x.Id.Value == shopId, cancellationToken)).FirstOrDefault() ??
              throw new ShopNotFoundException(shopId);
        var item = potentialShop.ItemsShop.FirstOrDefault(x => x.Id.Value == itemId) ??
            throw new ItemNotFoundException(itemId);

        if ((item.Amount - itemShop.Amount) >= 0)
            item.Amount -= itemShop.Amount;
        else
            throw new ItemOutOfStockException(itemId, itemShop.Amount);

        if (item.Amount == 0)
            potentialShop.ItemsShop.Remove(item);

        await Repository.Update(potentialShop, cancellationToken);
    }

    public async Task GetItems(Guid shopId, Guid storageId, Guid itemId, ItemShop itemShop, ItemStorage itemStorage, CancellationToken cancellationToken = default)
    {
        var potentialShop = (await Repository.Get(x => x.Id.Value == shopId, cancellationToken)).FirstOrDefault() ??
            throw new ShopNotFoundException(shopId);

        var potentialStorage = (await StorageRepository.GetWithoutTracking(x => x.Id.Value == storageId, cancellationToken)).FirstOrDefault() ??
                  throw new StorageNotFoundException(storageId);

        var itemFromShop = potentialShop.ItemsShop.FirstOrDefault(x => x.Item == itemShop.Item);


        if (itemFromShop is null)
        {
            if ((itemStorage.Amount - itemShop.Amount ) >= 0)
            {
                itemStorage.Amount -= itemShop.Amount;
                potentialStorage.ItemsStorage.Remove(itemStorage);
                itemFromShop.Amount += potentialShop.Capacity; 
            }
            else
                throw new ItemOutOfStockException(itemId, itemStorage.Amount);
        }

        await Repository.Update(potentialShop, cancellationToken);
        await StorageRepository.Update(potentialStorage, cancellationToken);
    }


    public async Task TransferItemsToShopAsync(Guid shopId, Guid itemId, int amount, ItemStorage itemStorage, ItemShop itemShop, CancellationToken cancellationToken = default)
    {
        var shop = (await Repository.GetWithoutTracking(x => x.Id.Value == shopId, cancellationToken)).FirstOrDefault() ??
                   throw new ShopNotFoundException(shopId);

        var requiredAmount = amount;

        var suitableStorages = await StorageRepository.GetWithoutTracking(x =>
            x.ItemsStorage.Any(x => x.Item.Id.Value == itemId && x.Amount >= amount) &&
            x.Temperature <= shop.Storages.FirstOrDefault()?.Temperature, cancellationToken);

        foreach (var storage in suitableStorages)
        {
            if (requiredAmount <= 0)
                break;


            if (itemStorage != null)
            {
                var transferAmount = Math.Min(itemStorage.Amount, requiredAmount);
                await TransferItemAsync(itemStorage, storage, shop, transferAmount, cancellationToken);
                requiredAmount -= transferAmount;
                itemShop.Amount += transferAmount;
            }
            
        }

        if (requiredAmount > itemStorage.Amount)
        {
            throw new InsufficientStockException(itemId, amount, amount - requiredAmount);
        }
        await Repository.Update(shop, cancellationToken);
    }

    private async Task TransferItemAsync(ItemStorage itemStorage, Storage sourceStorage, Shop targetShop, int transferAmount, CancellationToken cancellationToken)
    {
        var itemInTargetShop = targetShop.ItemsShop.FirstOrDefault(x => x.Item.Id == itemStorage.Item.Id);

        
        if (itemStorage.Amount > transferAmount)
        {
            itemStorage.Amount -= transferAmount;
            sourceStorage.ItemsStorage.Remove(itemStorage);
        }

        await StorageRepository.Update(sourceStorage, cancellationToken);
        await Repository.Update(targetShop, cancellationToken);
    }

    public async Task<Shop> GetShopById(Guid shopId, CancellationToken cancellationToken)
    {
        var shop = (await Repository.Get(x => x.Id.Value == shopId, cancellationToken)).FirstOrDefault() ??
            throw new ShopNotFoundException(shopId);
        return shop;
    }

    public async Task DeleteShop(Guid shopId, CancellationToken cancellationToken)
    {
        var shop = (await Repository.Get(x => x.Id.Value == shopId, cancellationToken)).FirstOrDefault() ??
            throw new ShopNotFoundException(shopId);
        await Repository.Remove(shop, cancellationToken);
    }
}
