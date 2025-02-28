# URL Shortener

A modern URL shortening service built with .NET Aspire.

## Overview

This URL shortener uses .NET Aspire to orchestrate multiple services including the API, PostgreSQL database, and Redis cache.

## Tech Stack

- .NET 9 with Aspire for service orchestration
- PostgreSQL for persistent storage
- Redis for distributed caching
- Swagger/OpenAPI for API documentation

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [.NET Aspire workload](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/quickstart-build-your-first-aspire-app)

## Running the Application

### Setting Up .NET Aspire

1. Install the .NET Aspire workload if you haven't already:
   ```bash
   dotnet workload install aspire
   ```

### Running the Project

1. Clone the repository
   ```bash
   git clone https://github.com/yourusername/urlshortener.git
   cd urlshortener
   ```

2. Run the Aspire AppHost project
   ```bash
   dotnet run --project UrlShortener.AppHost
   ```

3. The Aspire dashboard will automatically open in your browser, showing:
   - The URL Shortener API service
   - PostgreSQL database
   - Redis cache
   - Health status of all services
   - Logs and telemetry

4. Access the URL Shortener API through the URL shown in the dashboard
   - Swagger UI is available at `/swagger` when running in Development mode

## API Endpoints

### Shorten a URL
```http
POST /shorten?url=https://example.com
```

Response:
```json
{
  "shortCode": "abc123"
}
```

### Access a shortened URL
```http
GET /{shortCode}
```
Redirects to the original URL if found, returns 404 if not found.

### List all URLs
```http
GET /all
```

Response displays all shortened URLs in the system.

## Project Structure

- **UrlShortener.Api**: Web API project with endpoints for URL shortening
- **UrlShortener.AppHost**: .NET Aspire application host that coordinates all services
- **UrlShortener.ServiceDefaults**: Common service defaults and configurations

## Aspire Components Used

- `builder.AddServiceDefaults()`: Adds standard service defaults
- `builder.AddNpgsqlDataSource("url-database")`: Configures and registers PostgreSQL
- `builder.AddRedisDistributedCache("redis")`: Configures and registers Redis
- `builder.Services.AddHybridCache()`: Configures hybrid caching (memory + distributed)

## Development Notes

- The Aspire dashboard provides comprehensive monitoring of all services
- Service discovery is handled automatically by Aspire
- Configuration for services is managed through the Aspire AppHost
