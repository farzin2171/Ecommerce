# Product.Api — Technical Specification

## 1. Overview

`Product.Api` is the first microservice in an e-commerce platform built to practice microservices concepts. This initial service is intentionally simple: a single `Product` entity exposed through a standard CRUD REST API.

**Scope of this iteration:** one entity, full CRUD, clean layering. No cross-service communication, messaging, or authentication yet (deferred to later services).

## 2. Tech Stack & Constraints

| Concern | Decision |
|---|---|
| Framework | .NET 10 (ASP.NET Core Web API) |
| API style | Controller-based (**not** Minimal API) |
| Architecture | Layered + Repository pattern (**no** CQRS) |
| ORM | EF Core |
| Database | PostgreSQL (Npgsql provider) |
| API docs | Swagger / OpenAPI (Swashbuckle) |
| Source layout | All projects under `src/` |

## 3. Solution Structure

```
ecommerce/
├── src/
│   ├── Product.Api/                 # Web API (controllers, DI, Program.cs)
│   ├── Product.Application/         # Service layer, DTOs, interfaces
│   ├── Product.Domain/              # Entities, domain abstractions
│   └── Product.Infrastructure/      # EF Core, DbContext, repositories, migrations
├── tests/
│   └── Product.UnitTests/           # (optional, recommended)
├── Ecommerce.sln
└── docker-compose.yml               # PostgreSQL for local dev
```

**Project dependencies (reference direction):**

```
Product.Api ──> Product.Application ──> Product.Domain
     │                                        ▲
     └──────> Product.Infrastructure ─────────┘
```

`Product.Api` wires up DI for both `Application` and `Infrastructure`. `Infrastructure` and `Application` depend on `Domain`. `Domain` depends on nothing.

## 4. Domain Model

### Product entity

| Field | Type | Notes |
|---|---|---|
| `Id` | `Guid` | Primary key, generated on create |
| `Name` | `string` | Required, max 200 |
| `Description` | `string?` | Optional, max 2000 |
| `Price` | `decimal` | Required, >= 0, precision (18,2) |
| `Sku` | `string` | Required, unique, max 50 |
| `StockQuantity` | `int` | Required, >= 0, default 0 |
| `IsActive` | `bool` | Default true |
| `CreatedAtUtc` | `DateTime` | Set on create |
| `UpdatedAtUtc` | `DateTime?` | Set on update |

## 5. API Contract

Base route: `/api/products`

| Method | Route | Description | Success | Errors |
|---|---|---|---|---|
| GET | `/api/products` | List products (paged) | 200 | — |
| GET | `/api/products/{id}` | Get by id | 200 | 404 |
| POST | `/api/products` | Create | 201 + Location | 400 |
| PUT | `/api/products/{id}` | Full update | 204 | 400, 404 |
| DELETE | `/api/products/{id}` | Delete | 204 | 404 |

### DTOs

**ProductResponse** — mirrors entity fields (all of section 4).

**CreateProductRequest**
```
Name (required), Description?, Price (required), Sku (required), StockQuantity, IsActive
```

**UpdateProductRequest**
```
Name (required), Description?, Price (required), Sku (required), StockQuantity, IsActive
```

### List query params
`?pageNumber=1&pageSize=20` — return a paged result (`items`, `totalCount`, `pageNumber`, `pageSize`).

### Validation rules
- `Name`: not empty, <= 200
- `Price`: >= 0
- `Sku`: not empty, <= 50, unique (409 Conflict on duplicate create)
- `StockQuantity`: >= 0

Use `ProblemDetails` (RFC 7807) for error responses.

## 6. Layer Responsibilities

**Product.Domain**
- `Product` entity
- `IProductRepository` interface

**Product.Application**
- `IProductService` + `ProductService` (orchestration, mapping, business rules)
- DTOs (request/response)
- Validation (FluentValidation or DataAnnotations)
- Mapping (manual or AutoMapper/Mapster)

**Product.Infrastructure**
- `ProductDbContext` (EF Core, Npgsql)
- `ProductRepository : IProductRepository`
- Entity configuration (`IEntityTypeConfiguration<Product>`)
- EF Core migrations
- `AddInfrastructure(IServiceCollection, IConfiguration)` extension

**Product.Api**
- `ProductsController`
- `Program.cs` (DI, Swagger, middleware pipeline)
- `appsettings.json` (connection string)
- Exception-handling middleware

## 7. Repository Interface

```csharp
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, CancellationToken ct = default);
    Task<bool> SkuExistsAsync(string sku, CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    void Update(Product product);
    void Remove(Product product);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

## 8. Database & EF Core

- Provider: `Npgsql.EntityFrameworkCore.PostgreSQL`
- Connection string key: `ConnectionStrings:ProductDb`
- Configure `Product` via `IEntityTypeConfiguration<Product>`:
  - PK on `Id`
  - Unique index on `Sku`
  - `Price` precision `(18,2)`
  - Max lengths per section 4
- Apply migrations on startup in Development (or run manually).

**Local connection string example**
```
Host=localhost;Port=5432;Database=productdb;Username=postgres;Password=postgres
```

## 9. docker-compose (PostgreSQL)

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

## 10. Setup Commands

```bash
# Solution + structure
dotnet new sln -n Ecommerce
mkdir -p src

dotnet new webapi -n Product.Api          -o src/Product.Api --use-controllers
dotnet new classlib -n Product.Application -o src/Product.Application
dotnet new classlib -n Product.Domain      -o src/Product.Domain
dotnet new classlib -n Product.Infrastructure -o src/Product.Infrastructure

# Add to solution
dotnet sln add src/**/*.csproj

# Project references
dotnet add src/Product.Application reference src/Product.Domain
dotnet add src/Product.Infrastructure reference src/Product.Domain
dotnet add src/Product.Api reference src/Product.Application src/Product.Infrastructure

# Packages
dotnet add src/Product.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/Product.Infrastructure package Microsoft.EntityFrameworkCore.Design
dotnet add src/Product.Api package Swashbuckle.AspNetCore
# Optional: dotnet add src/Product.Application package FluentValidation.AspNetCore

# DB up
docker compose up -d

# Migration
dotnet ef migrations add InitialCreate \
  --project src/Product.Infrastructure \
  --startup-project src/Product.Api
dotnet ef database update \
  --project src/Product.Infrastructure \
  --startup-project src/Product.Api

# Run
dotnet run --project src/Product.Api
```

## 11. Swagger

- Enable Swashbuckle in `Program.cs`.
- Serve Swagger UI at `/swagger` (consider enabling in all environments early on).
- Annotate controller actions with `[ProducesResponseType]` for each status code in section 5.

## 12. Acceptance Criteria

- [ ] Solution builds with the four projects under `src/`.
- [ ] All five CRUD endpoints work against PostgreSQL.
- [ ] Repository pattern used; no EF Core types leak into the controller.
- [ ] No CQRS, no Minimal API.
- [ ] Swagger UI lists all endpoints with correct response types.
- [ ] Duplicate SKU on create returns 409.
- [ ] Unknown id returns 404 on GET/PUT/DELETE.
- [ ] Validation failures return 400 with `ProblemDetails`.

## 13. Out of Scope (future services / iterations)

Authentication/authorization, inter-service communication, messaging/events, API gateway, caching, pagination cursors, soft-delete, audit logging.
