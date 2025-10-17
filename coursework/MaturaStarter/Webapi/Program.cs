using DataAccess;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire services
builder.AddServiceDefaults(); // auto logging

builder.AddSqliteDbContext<ApplicationDataContext>("sqlite-db");

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/", async (ApplicationDataContext context) =>
{
    var customers = await context.Customers.ToListAsync();
    return Results.Ok(customers);
});

app.MapGet("/exception", () => {
    throw new Exception("Test exception");
});



app.Run();
