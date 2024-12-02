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

public static class ShopEndPoint
{
    public static void MapShopEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("api/shop/create", CreateShop).WithTags("Shop");
        routes.MapGet("api/shops", GetShop).WithTags("Shop");
        routes.MapGet("api/shop/item/{itemId}", GetItemById).WithTags("Item");
        routes.MapDelete("api/shop/{shopId}", DeleteShop).WithTags("Shop");
    }

    private static async Task<IResult> GetItemById(Guid shopId, Guid itemId, [FromBody] ItemShop shop, [FromServices] ShopService service, [FromServices] ILogger<ShopService> logger, CancellationToken cancellationToken)
    {
        try
        {
            await service.GetItemFromShop(shopId, itemId, shop, cancellationToken);
            return Results.Ok();
        }
        catch (ShopNotFoundException ex)
        {
            logger.LogError(ex, "Failed to find shop.");
            return Results.BadRequest($"Shop with ID '{shopId}' is not found.");
        }
        catch (ItemNotFoundException ex)
        {
            logger.LogError(ex, "Failed to find item.");
            return Results.BadRequest($"An item with ID '{itemId}' is not found.");
        }
    }

    [HttpPost]
    public static async Task<IResult> CreateShop([FromBody] ShopDTO shop, [FromServices] ShopService service, [FromServices] ILogger<ShopService> logger)
    {
        try
        {
            await service.CreateOrUpdateShopAsync(shop);
            return Results.Created($"api/items/{shop.Id}", shop);
        }
        catch (SimiliarItemException ex)
        {
            logger.LogError(ex, "Failed to create shop due to similar name.");
            return Results.BadRequest($"Shop with the name '{shop.Name}' already exists.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating the shop.");
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet]
    public static async Task<IResult> GetShop(Guid shopId, [FromServices] ShopService service, [FromServices] ILogger<ShopService> logger, CancellationToken cancellationToken)
    {
        try
        {
            await service.GetShopById(shopId, cancellationToken);
            return Results.Ok();
        }
        catch (ItemNotFoundException ex)
        {
            logger.LogError(ex, "Failed to find shop.");
            return Results.BadRequest($"Shop with ID '{shopId}' is not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating the shop.");
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete]
    public static async Task<IResult> DeleteShop(Guid shopId, [FromServices] ShopService service, [FromServices] ILogger<ShopService> logger, CancellationToken cancellationToken)
    {
        try
        {
            await service.DeleteShop(shopId, cancellationToken);
            return Results.NoContent();
        }
        catch (ItemNotFoundException ex)
        {
            logger.LogError(ex, "Failed to find shop.");
            return Results.BadRequest($"Shop with ID '{shopId}' is not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating the shop.");
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}


