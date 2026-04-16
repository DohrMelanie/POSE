using System.Data;
using AppServices;
using Microsoft.EntityFrameworkCore;

namespace WebApi;

// TODO: Add at least two meaningful integration tests for the order endpoints in WebApiTests/OrderIntegrationTests.cs

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/orders", GetAllOrders)
            .WithName("GetOrders")
            .Produces<List<OrderDto>>(StatusCodes.Status200OK);
        
        app.MapGet("/orders/{id:int}", GetOrderById)
            .WithName("GetOrderById")
            .Produces<OrderDetailDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        
        app.MapPost("/orders", CreateOrder)
            .WithName("CreateOrder")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
        
        app.MapGet("/bracelets/validate/{data}", ValidateBracelet)
            .WithName("ValidateBracelet")
            .Produces<ValidationResult>(StatusCodes.Status200OK);
        
        return app;
    }


    private static async Task<IResult> GetAllOrders(ApplicationDataContext db, int? minCost)
    {
        var orders = await db.Orders
            .Include(o => o.OrderItems)
            .Select(o => new OrderDto(
                o.Id,
                o.CustomerName,
                o.OrderDate,
                o.OrderItems.Sum(i => i.Costs),
                o.OrderItems.Count()
            ))
            .ToListAsync();
        
        var ordersOrdered = orders.OrderBy(o => o.OrderDate).ToList();
        
        if (minCost.HasValue)
        {
            ordersOrdered = ordersOrdered.FindAll(o => o.TotalCost >= minCost);
        }
        
        return Results.Ok(ordersOrdered);
    }

    private static async Task<IResult> GetOrderById(ApplicationDataContext db, int id)
    {
        var order = await db.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return Results.NotFound();
        }

        var detailDto = new OrderDetailDto(
            order.Id, 
            order.CustomerName, 
            order.OrderDate,
            order.TotalCosts, 
            order.OrderItems.Count, 
            order.OrderItems
                .Select(i => new BraceletDto(i.Id, i.BraceletData, i.Costs))
                .ToList());
        
        return Results.Ok(detailDto);
    }

    private static async Task<IResult> CreateOrder(ApplicationDataContext db, CreateOrderDto order, IBraceletSerializer serializer)
    {
        if (order.CustomerName.Length == 0
            || order.Address.Length == 0
            || !order.BraceletData.Any())
        {
            return Results.BadRequest();
        }
        var items = new List<OrderItem>();
        var total = 0m;

        foreach (var data in order.BraceletData)
        {
            var result = serializer.Parse(data, out var bracelet);

            if (result == BraceletValidationResult.Ok)
            {
                items.Add(new OrderItem
                {
                    BraceletData = data,
                    Costs = bracelet!.Cost
                });
                total += bracelet.Cost;
            }
            else
            {
                return Results.BadRequest();
            }
        }
        
        await db.Orders.AddAsync(new Order
        {
            CustomerName =  order.CustomerName,
            CustomerAddress = order.Address,
            OrderItems = items,
            TotalCosts = total,
            OrderDate = DateTime.Now
        });

        await db.SaveChangesAsync();

        return Results.Created();
    }

    private static async Task<IResult> ValidateBracelet(ApplicationDataContext db, IBraceletSerializer serializer, string data)
    {
        var result = serializer.Parse(data, out var bracelet);
        return Results.Ok(result == BraceletValidationResult.Ok ? 
            new ValidationResult(null, bracelet.HasMixedColors, bracelet.Cost) : 
            new ValidationResult(result.ToString(), false, null));
    }
}

public record OrderDto(
    int Id,
    string CustomerName,
    DateTime OrderDate,
    decimal TotalCost,
    int NumberOfBracelets
);

public record OrderDetailDto(
    int Id,
    string CustomerName,
    DateTime OrderDate,
    decimal TotalCost,
    int NumberOfBracelets,
    List<BraceletDto> OrderItems
);

public record BraceletDto(
    int Id,
    string BraceletData,
    decimal Cost
);

public record CreateOrderDto(
    string CustomerName,
    string Address,
    List<string> BraceletData
    );

public record ValidationResult(
    string? Error,
    bool MixedColorWarning,
    decimal? Cost);

// TODO: Add record type for DTOs here

/*
    Get all orders — list orders with optional minimum cost filter, ordered by date descending. 
    Return id, customer name, order date, total costs, and number of bracelets.
   
    Get order by ID — return full order details including all bracelet items. Return 404 if not found.
   
    Create order — accept customer name, address, and a list of bracelet data strings. 
    Validate all inputs (name/address required, at least one bracelet, all bracelets must be valid). Calculate costs server-side.
    
    Validate bracelet — accept a bracelet data string, return validation errors, mixed-color warning flag, and cost (if valid).
*/