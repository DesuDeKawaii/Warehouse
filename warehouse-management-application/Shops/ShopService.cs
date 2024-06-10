using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using warehouse_management_core.DTO_s;
using warehouse_management_core.Entities;

namespace warehouse_management_application.Shops;

public class ShopService(IRepository<Shop> repository) : IServise
{
    private IRepository<Shop> Repository { get; init; } = repository;

    public async Task<IEnumerable<ShopDTO>> GetShopAsync(CancellationToken cancellationToken = default) =>
        (await Repository.Get(cancellationToken)).Select(x => (ShopDTO)x);

    public async Task<IEnumerable<ShopDTO>> GetShopsItemsAsync(Guid shopId, CancellationToken cancellationToken = default)
    {
        var potentialShop = (await Repository.GetWithoutTracking(x => x.Id.Value == shopId, cancellationToken)).FirstOrDefault() ??
            throw new ShopNotFoundException(shopId);
        return (IEnumerable<ShopDTO>)potentialShop.ItemsShop.Cast<ItemDTO>();
    }

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

        var potentialStorage = (await Repository.GetWithoutTracking(x => x.Id.Value == storageId, cancellationToken)).FirstOrDefault() ??
                  throw new StorageNotFoundException(storageId);

        var itemFromShop = potentialShop.ItemsShop.FirstOrDefault(x => x.Item == itemShop.Item);


        if (itemFromShop is null)
        {
            if ((itemShop.Amount - itemStorage.Amount) >= 0)
                itemShop.Amount -= itemStorage.Amount;
            else
                throw new ItemOutOfStockException(itemId, itemStorage.Amount);

            potentialShop.ItemsShop.Add(new()
            {
                Item = itemShop.Item,
                Amount = itemShop.Amount
            });
        }
        else
            itemFromShop.Amount += itemShop.Amount;

        await Repository.Update(potentialShop, cancellationToken);
    }
}
