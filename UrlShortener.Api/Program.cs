using Microsoft.OpenApi.Models;
using UrlShortener.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDataSource("url-database");

builder.AddRedisDistributedCache("redis");

#pragma warning disable EXTEXP0018
builder.Services.AddHybridCache();
#pragma warning restore EXTEXP0018

builder.Services.AddHostedService<DatabaseInitializer>();
builder.Services.AddScoped<UrlshorteningService>();

// Add to services
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
    );
});

// Add Swagger services
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "URL Shortener API", Version = "v1" });
});

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        c.ConfigObject.DisplayRequestDuration = true;
    });
    app.MapOpenApi();

    // Add this to verify OpenAPI spec
    app.MapGet("/swagger/v1/swagger.json", () => Results.File("swagger/v1/swagger.json"));
}

app.MapPost(
    "shorten",
    async (string url, UrlshorteningService service) =>
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return Results.BadRequest("Invalid URL");
        }

        var shortCode = await service.ShortenUrl(url);

        return Results.Ok(new { shortCode });
    }
);

app.MapGet(
    "{shortCode}",
    async (string shortCode, UrlshorteningService service) =>
    {
        var originalUrl = await service.GetOriginalUrl(shortCode);

        if (originalUrl is null)
        {
            return Results.NotFound();
        }

        return Results.Redirect(originalUrl);

    }
);

app.MapGet(
    "all",
    async (UrlshorteningService service) =>
    {
        var urls = await service.GetAllUrls();

        return Results.Ok(urls);
    }
);

app.UseHttpsRedirection();

app.Run();
