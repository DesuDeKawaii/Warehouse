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







    public List<Tuple<int, int>> ExecuteTransportTask(List<Storage> storages, Shop shop)
    {
        var table = PrepareTable(storages, shop);
        table = NormalizeTable(table);
        SolveTransportTask(table);
        return GetOptimalDistribution(table, storages);
    }

    private List<List<Tuple<int, int>>> PrepareTable(List<Storage> storages, Shop shop)
    {
        var table = new List<List<Tuple<int, int>>>();

        foreach (var storage in storages)
        {
            var row = new List<Tuple<int, int>>
            {
                new Tuple<int, int>(CalculateCost(storage, shop), -1),
                new Tuple<int, int>(storage.ItemsStorage.Sum(x => x.Amount), -1)
            };
            table.Add(row);
        }

        var lastRow = new List<Tuple<int, int>>
        {
            new Tuple<int, int>(shop.ItemsShop.Sum(x => x.Amount), -1),
            new Tuple<int, int>(0, -1)
        };
        table.Add(lastRow);

        return table;
    }

    private int CalculateCost(Storage storage, Shop shop)
    {
        var distance = CalculateDistance(storage.Latitude, storage.Longitude, shop.Latitude, shop.Longitude);
        return (int)Math.Round(distance);
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var R = 6371;
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distance = R * c;
        return distance;
    }

    private double ToRadians(double angle)
    {
        return Math.PI * angle / 180.0;
    }

    private List<List<Tuple<int, int>>> NormalizeTable(List<List<Tuple<int, int>>> table)
    {
        int sumRow = table.Last().First().Item1;
        int sumCol = table.Sum(row => row.Last().Item1);

        if (sumCol < sumRow)
        {
            var newRow = new List<Tuple<int, int>>();
            for (int i = 0; i < table[0].Count - 1; i++)
            {
                newRow.Add(new Tuple<int, int>(0, -1));
            }
            newRow.Add(new Tuple<int, int>(sumRow - sumCol, -1));
            table.Insert(table.Count - 1, newRow);
        }
        return table;
    }

    private void SolveTransportTask(List<List<Tuple<int, int>>> table)
    {
        while (!IsSolved(table))
        {
            var minId = SearchMinCostCell(table);
            var min = Math.Min(table[minId.Item1].Last().Item1, table.Last()[minId.Item2].Item1);

            if (table[minId.Item1].Last().Item1 - min == 0) // row
            {
                for (int i = 0; i < table[minId.Item1].Count - 1; i++)
                {
                    if (minId.Item2 == i)
                    {
                        table[minId.Item1][i] = new Tuple<int, int>(table[minId.Item1][i].Item1, min);
                        continue;
                    }
                    if (table[minId.Item1][i].Item2 != -1) continue;
                    table[minId.Item1][i] = new Tuple<int, int>(table[minId.Item1][i].Item1, -2);
                }
            }
            else // column
            {
                for (int i = 0; i < table.Count - 1; i++)
                {
                    if (minId.Item1 == i)
                    {
                        table[i][minId.Item2] = new Tuple<int, int>(table[i][minId.Item2].Item1, min);
                        continue;
                    }
                    if (table[i][minId.Item2].Item2 != -1) continue;
                    table[i][minId.Item2] = new Tuple<int, int>(table[i][minId.Item2].Item1, -2);
                }
            }
            table[minId.Item1][table[0].Count - 1] = new Tuple<int, int>(table[minId.Item1][table[0].Count - 1].Item1 - min, -1);
            table[table.Count - 1][minId.Item2] = new Tuple<int, int>(table[table.Count - 1][minId.Item2].Item1 - min, -1);
        }
    }

    private Tuple<int, int> SearchMinCostCell(List<List<Tuple<int, int>>> table)
    {
        int min = int.MaxValue;
        Tuple<int, int> minId = null;
        for (int i = 0; i < table.Count - 1; i++)
        {
            for (int j = 0; j < table[i].Count - 1; j++)
            {
                if (table[i][j].Item1 < min && table[i][j].Item1 != 0 && table[i][j].Item2 == -1)
                {
                    minId = new Tuple<int, int>(i, j);
                    min = table[i][j].Item1;
                }
            }
        }
        if (min == int.MaxValue)
        {
            for (int i = 0; i < table.Count - 1; i++)
            {
                for (int j = 0; j < table[i].Count - 1; j++)
                {
                    if (table[i][j].Item1 == 0 && table[i][j].Item2 == -1)
                    {
                        minId = new Tuple<int, int>(i, j);
                        return minId;
                    }
                }
            }
        }
        return minId;
    }

    private bool IsSolved(List<List<Tuple<int, int>>> table)
    {
        return table.Last().All(t => t.Item1 == 0) && table.All(row => row.Last().Item1 == 0);
    }

    private List<Tuple<int, int>> GetOptimalDistribution(List<List<Tuple<int, int>>> table, List<Storage> storages)
    {
        var result = new List<Tuple<int, int>>();

        for (int i = 0; i < table.Count - 1; i++)
        {
            for (int j = 0; j < table[i].Count - 1; j++)
            {
                if (table[i][j].Item2 > 0)
                {
                    result.Add(new Tuple<int, int>(storages[i].Id, table[i][j].Item2));
                }
            }
        }
        return result;
    }


}
