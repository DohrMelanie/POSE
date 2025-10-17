var builder = DistributedApplication.CreateBuilder(args);

builder.Build().Run();

// e.g. we have sqlite, we have a web api that uses sqlite, we have a 2nd api that uses the first
// Aspire is based on docker