using System.Net.Mime;
using AppServices;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApi;

public static class WishListEndpoints
{
    public static IEndpointRouteBuilder MapWishListEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/verify-pin/{name}",
                async (ApplicationDataContext db, string name, [FromBody] AuthReq authReq) =>
                await HandleVerifyPin(db, name, authReq))
            .WithDescription("Check if the pin is from a child or a parent")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/wishlist/{name}/items",
                async (ApplicationDataContext db, string name, [FromBody] AuthReq authReq) =>
                await HandleRetrieveWishlistItems(db, name, authReq))
            .Produces(StatusCodes.Status200OK);

        app.MapPost("/wishlist/{name}/items/{itemId:int}/mark-as-bought",
            async (ApplicationDataContext db, string name, int itemId, [FromBody] AuthReq authReq) =>
            await HandleMarkAsBought(db, name, itemId, authReq));

        app.MapDelete("/wishlist/{name}/items/{itemId}",
            async (ApplicationDataContext db, string name, int itemId, [FromBody] AuthReq authReq) =>
            await HandleDeleteItem(db, name, itemId, authReq));

        app.MapPost("/wishlist/{name}/items/add",
            async (ApplicationDataContext db, string name, [FromBody] AddItemReq addItemReq) =>
            HandleAddItem(db, name, addItemReq));

        app.MapGet("/gift-categories",
            async (ApplicationDataContext db) => db.GiftCategories.Select(c => c.Name).ToList());

        return app;
    }

    private static async Task<IResult> HandleVerifyPin(ApplicationDataContext db, string pin, AuthReq authReq)
    {
        var wishlist = await db.Wishlists.FirstOrDefaultAsync(w => w.ParentPin == pin || w.ChildPin == pin);

        if (wishlist == null)
        {
            return Results.NotFound();
        }

        if (authReq.Pin != wishlist.ParentPin && authReq.Pin != wishlist.ChildPin)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new VerifyPinResp(GetRole(wishlist, authReq) == Role.Parent ? "parent" : "child"));
    }

    private static async Task<IResult> HandleRetrieveWishlistItems(ApplicationDataContext db, string name,
        AuthReq authReq)
    {
        var wishlist = db.Wishlists.Include(wishlist => wishlist.Items).FirstOrDefault(w => w.Name == name);
        if (wishlist == null)
        {
            return Results.NotFound($"Wishlist {name} not found.");
        }

        if (wishlist.ParentPin != authReq.Pin)
        {
            return Results.Unauthorized();
        }
        
        await db.Entry(wishlist).Collection(w => w.Items).LoadAsync();

        return Results.Ok(wishlist.Items);
    }

    private static async Task<IResult> HandleMarkAsBought(ApplicationDataContext db, string name, int itemId,
        AuthReq authReq)
    {
        var wishlist = db.Wishlists.Include(wishlist => wishlist.Items).FirstOrDefault(w => w.Name == name);
        if (wishlist == null)
        {
            return Results.NotFound($"Wishlist {name} not found.");
        }

        if (wishlist.ParentPin != authReq.Pin)
        {
            return Results.Unauthorized();
        }

        wishlist.Items.First(i => i.Id == itemId).Bought = true;
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> HandleDeleteItem(ApplicationDataContext db, string name, int itemId,
        AuthReq authReq)
    {
        var wishlist = db.Wishlists.Include(wishlist => wishlist.Items).FirstOrDefault(w => w.Name == name);
        if (wishlist == null)
        {
            return Results.NotFound($"Wishlist {name} not found.");
        }

        if (wishlist.ParentPin != authReq.Pin)
        {
            return Results.Unauthorized();
        }

        await db.WishlistItems
            .Where(i => i.Id == itemId)
            .ExecuteDeleteAsync();

        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    private static async Task<IResult> HandleAddItem(ApplicationDataContext db, string name, AddItemReq addItemReq)
    {
        var wishlist = db.Wishlists.FirstOrDefault(w => w.Name == name);
        if (wishlist == null)
        {
            return Results.NotFound($"Wishlist {name} not found.");
        }

        if (wishlist.ParentPin != addItemReq.Pin && wishlist.ChildPin != addItemReq.Pin)
        {
            return Results.Unauthorized();
        }

        await db.WishlistItems.AddAsync(new WishlistItem
        {
            Bought = false,
            ItemName = addItemReq.ItemName,
            Category = await db.GiftCategories.FirstOrDefaultAsync(c => c.Name == addItemReq.Category) ??
                       new GiftCategory { Name = addItemReq.Category }
        });

        await db.SaveChangesAsync();
        return Results.Ok();
    }

    private static Role GetRole(Wishlist wishlist, AuthReq authReq)
    {
        return wishlist.ParentPin == authReq.Pin ? Role.Parent : Role.Child;
    }

    public record AuthReq(
        string WishListName,
        string Pin
    );

    public record VerifyPinResp(
        string role
    );

    public record AddItemReq(
        string ItemName,
        string Category,
        string Pin
    );

    public enum Role
    {
        Parent,
        Child
    };
}