using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var sqlite = builder.AddSqlite(
    "sqlite-db",
    builder.Configuration["Database:path"],
    builder.Configuration["Database:fileName"])
    .WithSqliteWeb(); // works just with docker

var webapi = builder.AddProject<Webapi>("webapi")
    .WithReference(sqlite);

builder.AddNpmApp("frontend", "../Frontend")
    .WithReference(webapi)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints();

builder.Build().Run();

// e.g. we have sqlite, we have a web api that uses sqlite, we have a 2nd api that uses the first
// Aspire is based on docker
