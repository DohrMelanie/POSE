using System.Net.Mime;
using AppServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApi;

public static class WishListEndpoints
{
    public static IEndpointRouteBuilder MapWishListEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/verify-pin/{name}", async (ApplicationDataContext db, string name, [FromBody] AuthReq authReq) => HandleVerifyPin(db, name, authReq));

        app.MapPost("/wishlist/{name}/items",
            async (ApplicationDataContext db, string name, [FromBody] AuthReq authReq) => HandleRetrieveWishlistItems(db, name, authReq));

        app.MapPost("/wishlist/{name}/items/{itemId:int}/mark-as-bought",
            async (ApplicationDataContext db, string name, int itemId, [FromBody] AuthReq authReq) => HandleMarkAsBought(db, name, itemId, authReq));

        app.MapDelete("/wishlist/{name}/items/{itemId}", async (ApplicationDataContext db, string name, int itemId, [FromBody] AuthReq authReq) => HandleDeleteItem(db, name, itemId, authReq));

        app.MapPost("/wishlist/{name}/items/add",
            async (ApplicationDataContext db, string name, [FromBody] AuthReq authReq) => HandleAddItem(db, name, authReq));

        app.MapGet("/gift-categories", async (ApplicationDataContext db) => HandleRetrieveGifts(db));

        return app;
    }

    private static Task<IResult> HandleVerifyPin(ApplicationDataContext db, string pin, AuthReq authReq)
    {/*
        var wishlist = db.Wishlists.FirstOrDefaultAsync(w => w.);

        if (authReq.WishListName != name ||
            (authReq.Pin != wishlist.Result?.ParentPin && authReq.Pin != wishlist.Result?.ChildPin))
        {
            return Results.Unauthorized();
        }
        if (wishlist.Result?.ParentPin == authRe)*/
        throw new NotImplementedException();
        
    }

    private static Task HandleRetrieveWishlistItems(ApplicationDataContext db, string name, AuthReq authReq)
    {
        throw new NotImplementedException();
    }

    private static Task HandleMarkAsBought(ApplicationDataContext db, string name, int itemId, AuthReq authReq)
    {
        throw new NotImplementedException();
    }

    private static Task HandleDeleteItem(ApplicationDataContext db, string name, int itemId, AuthReq authReq)
    {
        throw new NotImplementedException();
    }

    private static Task HandleAddItem(ApplicationDataContext db, string name, AuthReq authReq) 
    {
        throw new NotImplementedException();
    }
    
    private static Task HandleRetrieveGifts(ApplicationDataContext db)
    {
        throw new NotImplementedException();
    }

    public record AuthReq(
        string WishListName,
        string Pin
    );
}