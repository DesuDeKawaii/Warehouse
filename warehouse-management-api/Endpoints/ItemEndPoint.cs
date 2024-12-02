using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using warehouse_management_application.Items;
using warehouse_management_core.DTO_s;
using warehouse_management_core.Entities;
using warehouse_management_core.Exceptions;

namespace warehouse_management_api.Endpoints;

public static class ItemEndPoint
{
    public static void MapItemEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("api/item/create", CreateItem).WithTags("Item");
        routes.MapGet("api/items", GetItem).WithTags("Item");
        routes.MapGet("api/item/{itemId}", GetItemById).WithTags("Item");
        routes.MapDelete("api/item/{itemId}", DeleteItem).WithTags("Item");
    }

    public static async Task<IResult> CreateItem(ItemDTO item, ItemService service, ILogger<ItemService> logger)
    {
        try
        {
            await service.CreateOrUpdateItemAsync(item);
            return Results.Created($"api/items/{item.Id}", item);
        }
        catch (SimiliarItemException ex)
        {
            logger.LogError(ex, "Failed to create item due to similar name.");
            return Results.BadRequest($"An item with the name '{item.Name}' already exists.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating the item.");
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    public static async Task<IResult> GetItem(ItemService service, CancellationToken cancellationToken)
    {
        try
        {
            var items = await service.GetItemsAsync(cancellationToken);
            return Results.Ok(items);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    public static async Task<IResult> GetItemById(Guid itemId, ItemService service, ILogger<ItemService> logger, CancellationToken cancellationToken)
    {
        try
        {
            var item = await service.GetItemById(itemId, cancellationToken);
            return Results.Ok(item);
        }
        catch (ItemNotFoundException ex)
        {
            logger.LogError(ex, "Failed to find item.");
            return Results.BadRequest($"An item with ID '{itemId}' is not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving the item.");
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    public static async Task<IResult> DeleteItem(Guid itemId, ItemService service, ILogger<ItemService> logger, CancellationToken cancellationToken)
    {
        try
        {
            await service.DeleteItemById(itemId, cancellationToken);
            return Results.NoContent();
        }
        catch (ItemNotFoundException ex)
        {
            logger.LogError(ex, "Failed to find item.");
            return Results.BadRequest($"An item with ID '{itemId}' is not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting the item.");
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}


