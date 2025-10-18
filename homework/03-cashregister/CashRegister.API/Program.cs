using CashRegister.API;
using CashRegister.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<ApplicationDataContext>();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CashRegisterCorsPolicy",
        policyBuilder =>
        {
            policyBuilder.WithOrigins("http://localhost:4200");
            policyBuilder.AllowAnyHeader();
            policyBuilder.AllowAnyMethod();
            policyBuilder.AllowCredentials();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapCashRegisterEndpoints();
app.UseHttpsRedirection();

app.Run();
