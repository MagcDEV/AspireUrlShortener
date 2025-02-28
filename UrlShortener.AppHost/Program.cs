var builder = DistributedApplication.CreateBuilder(args);

var postgresPassword = builder.AddParameter("postgres-password", secret: true);

var postgres = builder
    .AddPostgres("postgres", password: postgresPassword) // Store the postgres resource
    .WithDataVolume()
    .WithPgAdmin();

var urlDatabase = postgres.AddDatabase("url-database"); // Store the database resource

var redis = builder.AddRedis("redis");

builder
    .AddProject<Projects.UrlShortener_Api>("url-shortener-api")
    .WithReference(urlDatabase) // Reference the database resource, not just postgres
    .WithReference(redis)
    .WaitFor(postgres)
    .WaitFor(redis);

builder.Build().Run();
