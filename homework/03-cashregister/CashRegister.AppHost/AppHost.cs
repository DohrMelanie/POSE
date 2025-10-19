using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var sqlite = builder
    .AddSqlite(
        "sqlite-db",
        builder.Configuration["Database:path"],
        builder.Configuration["Database:fileName"])
    .WithSqliteWeb();

var webapi = builder
    .AddProject<CashRegister_API>("api")
    .WithReference(sqlite);

builder
    .AddNpmApp("frontend", "../Frontend")
    .WithReference(webapi)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints();

builder.Build().Run();
