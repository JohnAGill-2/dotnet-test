using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL database
var postgres = builder
    .AddPostgres("postgres")
    .WithImageTag("17")
    .WithPortBinding(5432, 5432)
    .WithEnvironment("POSTGRES_USER", "postgres")
    .WithEnvironment("POSTGRES_PASSWORD", "postgres")
    .WithEnvironment("POSTGRES_DB", "dotnet_test");

var postgresDb = postgres.AddDatabase("dotnet-test-dev");

// Add backend API
builder
    .AddProject("backend", "../backend/dotnet-test.csproj")
    .WithReference(postgresDb)
    .WithHttpEndpoint(port: 5041, targetPort: 5041, name: "http")
    .WithHttpsEndpoint(port: 7041, targetPort: 7041, name: "https");

builder.Build().Run();
