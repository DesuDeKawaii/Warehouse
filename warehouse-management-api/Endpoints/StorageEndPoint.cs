using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using warehouse_management_application.Items;
using warehouse_management_application.Shops;
using warehouse_management_application.Storages;
using warehouse_management_core.DTO_s;
using warehouse_management_core.Entities;
using warehouse_management_core.Exceptions;

namespace warehouse_management_api.Endpoints;

public static class StorageEndPoint
{
    public static void MapStorageEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("api/storage/create", CreateStorage).WithTags("Storage");
        routes.MapGet("api/storages", GetStorage).WithTags("Storage");
        routes.MapGet("api/storage/item/{itemId}", GetItemById).WithTags("Item");
        routes.MapDelete("api/storage/{storageId}", DeleteStorage).WithTags("Storage");
    }

    private static async Task<IResult> GetItemById(Guid itemId, [FromServices] StorageService service, [FromServices] ILogger<StorageService> logger, CancellationToken cancellationToken)
    {
        try
        {
            await service.GetStoragesItemsAsync(itemId, cancellationToken);
            return Results.Ok();
        }
        catch (ItemNotFoundException ex)
        {
            logger.LogError(ex, "Failed to find item.");
            return Results.BadRequest($"An item with ID '{itemId}' is not found.");
        }
        catch (ItemOutOfStockException ex)
        {
            logger.LogError(ex, "Item is out of stock.");
            return Results.BadRequest($"An item with ID '{itemId}' is out of stock.");
        }
    }

    public static async Task<IResult> CreateStorage(StorageDTO storage, [FromServices] StorageService service, [FromServices] ILogger<StorageService> logger)
    {
        try
        {
            await service.CreateOrUpdateStorageAsync(storage);
            return Results.Created($"api/storages/{storage.Id}", storage);
        }
        catch (SimiliarItemException ex)
        {
            logger.LogError(ex, "Failed to create storage due to similar name.");
            return Results.BadRequest($"Storage with the name '{storage.Name}' already exists.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating the storage.");
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    public static async Task<IResult> GetStorage(Guid storageId, [FromServices] StorageService service, [FromServices] ILogger<StorageService> logger, CancellationToken cancellationToken)
    {
        try
        {
            await service.GetStorageById(storageId, cancellationToken);
            return Results.Ok();
        }
        catch (ItemNotFoundException ex)
        {
            logger.LogError(ex, "Failed to find storage.");
            return Results.BadRequest($"Storage with ID '{storageId}' is not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving the storage.");
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    public static async Task<IResult> DeleteStorage(Guid storageId, [FromServices] StorageService service, [FromServices] ILogger<StorageService> logger, CancellationToken cancellationToken)
    {
        try
        {
            await service.DeleteStorageAsync(storageId, cancellationToken);
            return Results.NoContent();
        }
        catch (ItemNotFoundException ex)
        {
            logger.LogError(ex, "Failed to find storage.");
            return Results.BadRequest($"Storage with ID '{storageId}' is not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting the storage.");
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}

