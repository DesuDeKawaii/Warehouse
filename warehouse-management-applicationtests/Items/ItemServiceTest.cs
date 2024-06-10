using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using warehouse_management_application.Items;
using warehouse_management_core;
using warehouse_management_core.DTO_s;
using warehouse_management_core.Entities;
using warehouse_management_core.Exceptions;

namespace warehouse_management_applicationtests.Items;

public class ItemServiceTest
{
    [TestCase("name1", "Desc", "2020-01-01", 10, 10.99)]
    [TestCase("name2", "Desc", "2021-01-01", 9, 0.99)]
    [TestCase("name3", "Desc", "2022-01-01", 11, 1.99)]
    [Test]
    public async Task CreateOrUpdateItem_WhenItemForUpdateDoesntExists_ThrowsException()
    {
        // Arrange
        var repo = new FakeRepository<Item>();
        var item = new Item
        {
            Id = new Id(Guid.NewGuid()),
            Name = "Test"
        };
        await repo.AddRange([item]);

        var service = new ItemService(repo);
        var toChange = new Item { Id = new Id(Guid.NewGuid()), Name = "NewName" };

        // Act
        AsyncTestDelegate act = async delegate { await service.CreateOrUpdateItemAsync(toChange); };

        // Assert
        Assert.ThrowsAsync<ItemNotFoundException>(act);
    }
}

   
