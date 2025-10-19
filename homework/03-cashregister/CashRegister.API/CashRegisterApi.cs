namespace CashRegister.API;

public static class CashRegisterApi
{
    public static void MapCashRegisterEndpoints(this IEndpointRouteBuilder app) 
    {
        var group = app.MapGroup("/cash-register");
        group.MapGet("/items", CashRegisterEndpoints.GetItems)
            .WithName("GetItems")
            .WithDescription("Get all items")
            .Produces<List<Data.Item>>();

        group.MapPut("/checkout", CashRegisterEndpoints.Checkout)
            .WithName("Checkout")
            .WithDescription("Checkout items")
            .Produces<Data.Receipt>(201);
    }
}