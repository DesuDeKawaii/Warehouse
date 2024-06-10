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
    }



    [HttpPost]
    public static async Task<IResult> CreateItem(ItemDTO item, ItemService service, HttpContext context, ILogger logger)
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

    [HttpGet]
    public async static Task<IResult> GetItem(ItemService service, CancellationToken cancellationToken)
    {
        try
        {
            return Results.Ok(await service.GetItemsAsync(cancellationToken));
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    [HttpGet]
    public static async Task<IResult> GetItemById(Guid itemId, ItemService service, ILogger logger, CancellationToken cancellationToken)
    {
        try
        {
            await service.GetItemById(itemId, cancellationToken);
            return Results.Ok();

        }
        catch (ItemNotFoundException ex)
        {
            logger.LogError(ex, "Failed to find item.");
            return Results.BadRequest($"An item with ID '{itemId}' is not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating the item.");
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

}
