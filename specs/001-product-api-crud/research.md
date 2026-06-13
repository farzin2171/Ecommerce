# Research: Product API — CRUD Microservice

**Phase**: 0 — Outline & Research  
**Branch**: `001-product-api-crud`  
**Date**: 2026-06-12

## Research Topics

### 1. Layered Architecture + Repository Pattern in .NET 10 / ASP.NET Core

**Decision**: Four-project structure: `Domain` → `Application` → `Infrastructure` + `Api`. Repository interface lives in Domain; implementation in Infrastructure. Application service depends on the interface via constructor injection.

**Rationale**: This is the canonical Clean Architecture lite / Onion pattern for .NET microservices. The four-project split enforces compile-time dependency checking — if `Infrastructure` accidentally references `Api`, the build fails. EF Core types (DbContext, IQueryable) stay inside `Infrastructure` and never leak upward.

**Alternatives considered**:
- Single project: Simpler setup but no compile-time layering enforcement; unsuitable for a patterns-learning project.
- CQRS with MediatR: Adds indirection without clear benefit for a five-endpoint CRUD service. Explicitly ruled out by spec.

---

### 2. EF Core 9 + Npgsql on .NET 10

**Decision**: Use `Npgsql.EntityFrameworkCore.PostgreSQL` as the EF Core provider. Configure via `IEntityTypeConfiguration<Product>` in `Infrastructure`. Apply migrations on startup in Development (manual in other environments).

**Rationale**: Npgsql is the standard, well-maintained PostgreSQL provider for EF Core. `IEntityTypeConfiguration` keeps entity mapping out of `OnModelCreating`, which becomes unwieldy as the model grows.

**Key configuration decisions**:
- `Price`: `HasPrecision(18, 2)` to avoid floating-point rounding.
- `Sku`: `IsUnique()` index — enforced at the DB level in addition to the application-level check.
- Max lengths: applied via `HasMaxLength()` so EF Core migrations generate correct `VARCHAR(n)` columns.
- Timestamps: stored as `timestamp without time zone` (UTC); application always assigns UTC values.

**Alternatives considered**:
- Dapper: Lighter, but requires hand-written SQL and no migrations — higher boilerplate for a learning project.
- MongoDB: Not relational; not appropriate for a structured product catalog with typed constraints.

---

### 3. ProblemDetails (RFC 7807) for Error Responses

**Decision**: Use ASP.NET Core's built-in `ProblemDetails` infrastructure (`AddProblemDetails()` + `UseExceptionHandler()` middleware or a custom exception-handling middleware). Return `ValidationProblemDetails` for 400s and plain `ProblemDetails` for 404/409/500.

**Rationale**: RFC 7807 is the standard for HTTP error responses and is natively supported in ASP.NET Core 7+. Using built-in infrastructure avoids hand-rolling JSON error shapes and ensures consistent `type`, `title`, `status`, `detail` fields across all endpoints.

**Implementation pattern**:
```csharp
// Program.cs
builder.Services.AddProblemDetails();
app.UseExceptionHandler();

// Custom exception middleware maps domain exceptions to HTTP status codes:
//   ProductNotFoundException     → 404
//   DuplicateSkuException        → 409
//   ValidationException          → 400 + ValidationProblemDetails
```

**Alternatives considered**:
- Custom error envelope (`{ "errors": [...] }`): Non-standard; client tooling doesn't understand it.
- FluentValidation + `ValidationProblemDetails`: Recommended for richer validation messages; can be added later.

---

### 4. Pagination Pattern for REST APIs

**Decision**: Offset-based pagination via `?pageNumber=1&pageSize=20` query parameters. Response envelope:

```json
{
  "items": [...],
  "totalCount": 42,
  "pageNumber": 1,
  "pageSize": 20
}
```

**Rationale**: Offset pagination is the simplest approach and sufficient for a catalog with no real-time insertion pressure. A `PagedResult<T>` generic wrapper in the Application layer keeps the pattern reusable.

**Defaults and validation**:
- `pageNumber` defaults to 1 (minimum 1).
- `pageSize` defaults to 20 (minimum 1, maximum 100 — prevents runaway queries).
- If caller omits either parameter, defaults apply.

**Alternatives considered**:
- Cursor-based (keyset) pagination: More scalable at high volume but significantly more complex. Explicitly deferred by spec.
- Link headers (RFC 5988): Useful for HATEOAS; out of scope for this iteration.

---

### 5. Swagger / OpenAPI via Swashbuckle

**Decision**: Add `Swashbuckle.AspNetCore` to `Product.Api`. Enable Swagger UI at `/swagger` in all environments (convenient during development). Annotate controller actions with `[ProducesResponseType]` for each documented status code.

**Rationale**: Swashbuckle is the de-facto standard for ASP.NET Core OpenAPI documentation. Enabling it in all environments eliminates a common pitfall where developers forget to re-enable it locally after a production deploy toggle.

**Key annotations**:
- `[Produces("application/json")]` on controller class.
- `[ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]`
- `[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]`
- `[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]`

---

### 6. Duplicate SKU Detection

**Decision**: Check for SKU existence in the Application service layer before calling `AddAsync`, returning a `DuplicateSkuException` that maps to HTTP 409 Conflict. The database also enforces the unique index as a safety net.

**Rationale**: A pre-check in the service layer gives a cleaner error message than catching a database constraint violation. The DB unique index prevents races and acts as a last-resort guard.

**Pattern**:
```csharp
if (await _repository.SkuExistsAsync(request.Sku, ct))
    throw new DuplicateSkuException(request.Sku);
```

---

### 7. Docker Compose for Local PostgreSQL

**Decision**: Single `postgres:16` service in `docker-compose.yml` with a named volume for data persistence. Connection string key: `ConnectionStrings:ProductDb`.

**Standard local connection string**:
```
Host=localhost;Port=5432;Database=productdb;Username=postgres;Password=postgres
```

**Migration approach**: `dotnet ef database update` run manually, or `context.Database.MigrateAsync()` called at startup in Development environment.

## Resolved Clarifications

All technical decisions were derivable from the spec and standard .NET ecosystem practices. No open clarifications remain.
