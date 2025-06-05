# Docker Setup for Exchange Rates Frontend with ASP.NET Aspire

This document explains how to use Docker with this frontend application in an ASP.NET Aspire environment.

## Files Added

- `Dockerfile` - Multi-stage build file for creating an optimized production container
- `nginx.conf` - Custom Nginx configuration to handle SPA routing
- `docker-compose.yml` - Docker Compose configuration for local development/testing
- `.dockerignore` - Specifies which files to exclude from the Docker build context

## Building and Running with Docker

### Build the Docker image

```bash
docker build -t exchange-rates-front .
```

### Run the container

```bash
docker run -p 5173:80 exchange-rates-front
```

The application will be available at http://localhost:5173

## Using with ASP.NET Aspire

To integrate this frontend with an ASP.NET Aspire application:

1. Add this container to your Aspire AppHost project's Program.cs file:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Add the frontend container
var frontend = builder.AddContainer("frontend", "exchange-rates-front")
    .WithHttpEndpoint(containerPort: 80, name: "frontend")
    .WithEnvironment("API_URL", "{backend-service-url}");

// Add other Aspire services...

var app = builder.Build();
await app.RunAsync();
```

2. Make sure to replace `{backend-service-url}` with the actual URL of your backend service.

3. If you're using Docker Compose with Aspire, make sure the network names match between your Aspire project and this project's docker-compose.yml file.

## Configuration

You can configure the container by setting environment variables at runtime:

```bash
docker run -p 5173:80 -e API_URL=http://backend-api:8080 exchange-rates-front
```

## Health Checks

The container includes a health check endpoint at `/health` that returns HTTP 200 when the service is healthy.
