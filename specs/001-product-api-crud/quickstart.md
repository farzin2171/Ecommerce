# Quickstart: Product API — CRUD Microservice

**Branch**: `001-product-api-crud`  
**Date**: 2026-06-12

## Prerequisites

- .NET 10 SDK
- Docker Desktop (for PostgreSQL)
- `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`

## 1. Create Solution & Projects

```bash
# Solution
dotnet new sln -n Ecommerce

# Projects
dotnet new webapi    -n Product.Api            -o src/Product.Api            --use-controllers
dotnet new classlib  -n Product.Application    -o src/Product.Application
dotnet new classlib  -n Product.Domain         -o src/Product.Domain
dotnet new classlib  -n Product.Infrastructure -o src/Product.Infrastructure
dotnet new xunit     -n Product.UnitTests      -o tests/Product.UnitTests

# Add to solution
dotnet sln add src/**/*.csproj tests/**/*.csproj
```

## 2. Wire Project References

```bash
dotnet add src/Product.Application    reference src/Product.Domain
dotnet add src/Product.Infrastructure reference src/Product.Domain
dotnet add src/Product.Api            reference src/Product.Application src/Product.Infrastructure
dotnet add tests/Product.UnitTests    reference src/Product.Application src/Product.Domain
```

## 3. Install Packages

```bash
# Infrastructure
dotnet add src/Product.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/Product.Infrastructure package Microsoft.EntityFrameworkCore.Design

# API
dotnet add src/Product.Api package Swashbuckle.AspNetCore

# Tests
dotnet add tests/Product.UnitTests package Microsoft.EntityFrameworkCore.InMemory
```

## 4. Start PostgreSQL

```bash
docker compose up -d
```

Verify: `docker ps` — the `ecommerce-postgres` container should be running.

## 5. Apply Database Migrations

```bash
# Create initial migration
dotnet ef migrations add InitialCreate \
  --project src/Product.Infrastructure \
  --startup-project src/Product.Api

# Apply to database
dotnet ef database update \
  --project src/Product.Infrastructure \
  --startup-project src/Product.Api
```

## 6. Run the API

```bash
dotnet run --project src/Product.Api
```

Swagger UI: `http://localhost:<port>/swagger`

## 7. Smoke Test

```bash
# Create a product
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Test Product","price":9.99,"sku":"TEST-001","stockQuantity":10}'

# List products
curl http://localhost:5000/api/products

# Get by id (replace with actual id from create response)
curl http://localhost:5000/api/products/<id>
```

## Connection String

In `src/Product.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "ProductDb": "Host=localhost;Port=5432;Database=productdb;Username=postgres;Password=postgres"
  }
}
```

## docker-compose.yml

```yaml
services:
  postgres:
    image: postgres:16
    container_name: ecommerce-postgres
    environment:
      POSTGRES_DB: productdb
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data
volumes:
  pgdata:
```
