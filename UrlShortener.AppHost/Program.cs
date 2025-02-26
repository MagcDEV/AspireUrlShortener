var builder = DistributedApplication.CreateBuilder(args);

 var postgres = builder.AddPostgres("postgres").WithPgAdmin();

 var urlDatabase = postgres.AddDatabase("url-database"); // Store the database resource

 builder
     .AddProject<Projects.UrlShortener_Api>("url-shortener-api")
     .WithReference(urlDatabase) // Reference the database resource, not just postgres
     .WaitFor(postgres);

 builder.Build().Run();
