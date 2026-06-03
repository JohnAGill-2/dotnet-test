using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL container — Aspire pulls and manages the Docker image
var postgres = builder
    .AddPostgres("postgres")
    .WithImageTag("17")
    // .WithHostPort(5432); // Commenting out the unsupported call
    .WithEnvironment("POSTGRES_USER", "postgres")
    .WithEnvironment("POSTGRES_PASSWORD", "postgres")
    .WithEnvironment("POSTGRES_DB", "dotnet_test_dev");

var postgresDb = postgres.AddDatabase("dotnet-test-dev");

// Add backend API, wired to the Postgres database
builder
    .AddProject("backend", "../backend/dotnet-test.csproj")
    .WithReference(postgresDb)
    .WithHttpEndpoint(port: 5041, targetPort: 5041, name: "http");

builder.Build().Run();
