using CashRegister.API;
using CashRegister.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults(); // auto logging, etc.

builder.AddSqliteDbContext<ApplicationDataContext>("sqlite-db");

builder.Services.AddOpenApi();

builder.Services.AddCors();

var app = builder.Build();

app.UseCors(options =>
{
    options.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

}

app.MapCashRegisterEndpoints();
app.UseHttpsRedirection();

app.Run();
