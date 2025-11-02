using AppServices;

namespace WebApi;

public static class TodoListEndpoints
{
    public static IEndpointRouteBuilder MapTodoListEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/items", (ApplicationDataContext db) => db.TodoItems)
            .Produces<List<TodoItem>>()
            .WithName("GetAllTodoItems")
            .WithDescription("Gets all todo items the database.");

        app.MapPut("/items/{id:int}", async (int id, TodoItem updatedItem, ApplicationDataContext db) =>
        {
            var item = await db.TodoItems.FindAsync(id);
            if (item is null) return Results.NotFound();
            item.Assignee = updatedItem.Assignee;
            item.Title = updatedItem.Title;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
        
        app.MapPost("/items", async (TodoItem newItem, ApplicationDataContext db) =>
        {
            db.TodoItems.Add(newItem);
            await db.SaveChangesAsync();
            return Results.Created($"/items/{newItem.Id}", newItem);
        });
        
        app.MapDelete("/items/{id:int}", async (int id, ApplicationDataContext db) =>
        {
            var item = await db.TodoItems.FindAsync(id);
            if (item is null) return Results.NotFound();
            db.TodoItems.Remove(item);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

    // Demonstrates an endpoint that uses a service to perform some logic.
    // Receives an object, modifies it using the service, and returns the 
    // modified object.
    /*
    app.MapPost("/dummy-logic", async (ApplicationDataContext db, Dummy dummyToChange, IDummyLogic logic) =>
    {
        logic.IncrementDecimal(dummyToChange, 1.5m);
        return Results.Ok(dummyToChange);
    })
    .Produces<Dummy>(StatusCodes.Status200OK)
    .WithDescription("Increments the DecimalProperty of the provided Dummy object by 1.5 using the DummyLogic service.");
    app.MapPost("/generate", GenerateRecords)
    .Produces<List<DemoOutputDto>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest)
    .WithDescription("Generates a list of DemoOutputDto objects based on the specified number of records.");
    */
    return app;
    }
/*
    public static IResult GenerateRecords(DemoInputDto input)
    {
        if (input.NumberOfRecords < 1 || input.NumberOfRecords > 1000)
        {
            return Results.BadRequest("NumberOfRecords must be between 1 and 1000.");
        }

        var output = Enumerable.Range(1, input.NumberOfRecords)
            .Select(i => new DemoOutputDto(i, $"Name {i}"))
            .ToList();
        return Results.Ok(output);
    }*/
}

public record DemoInputDto(int NumberOfRecords);

public record DemoOutputDto(int Id, string Name);
