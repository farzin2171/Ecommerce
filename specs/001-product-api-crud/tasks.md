# Tasks: Product API — CRUD Microservice

**Input**: Design documents from `specs/001-product-api-crud/`  
**Prerequisites**: plan.md ✅ spec.md ✅ research.md ✅ data-model.md ✅ contracts/products-api.md ✅

**Organization**: Tasks are grouped by user story (US1–US5 map to spec.md priorities P1–P5). Each phase produces a complete, independently testable increment.

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel (different files, no dependencies on concurrent tasks)
- **[Story]**: Which user story this task belongs to (US1–US5)
- Paths are relative to the repository root

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the .NET solution, projects, references, packages, and Docker Compose file. No business logic yet.

- [X] T001 Create `Ecommerce.sln` and scaffold `src/Product.Api`, `src/Product.Application`, `src/Product.Domain`, `src/Product.Infrastructure` (classlib), and `tests/Product.UnitTests` (xunit) per quickstart.md
- [X] T002 Add project references: Application→Domain, Infrastructure→Domain, Api→Application+Infrastructure, UnitTests→Application+Domain
- [X] T003 [P] Install NuGet packages in `src/Product.Infrastructure`: `Npgsql.EntityFrameworkCore.PostgreSQL` and `Microsoft.EntityFrameworkCore.Design`
- [X] T004 [P] Install NuGet package in `src/Product.Api`: `Swashbuckle.AspNetCore`
- [X] T005 [P] Install NuGet package in `tests/Product.UnitTests`: `Microsoft.EntityFrameworkCore.InMemory`
- [X] T006 Create `docker-compose.yml` at repository root with `postgres:16` service, `ecommerce-postgres` container name, `productdb` database, and named `pgdata` volume per quickstart.md

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core domain model, persistence layer, DI wiring, and API skeleton. **No user story work can begin until this phase is complete.**

**⚠️ CRITICAL**: Phases 3–7 all depend on this phase being fully complete.

- [X] T007 Create `Product` entity class with all 9 fields from data-model.md in `src/Product.Domain/Entities/Product.cs`
- [X] T008 Create `IProductRepository` interface with all 7 method signatures from data-model.md in `src/Product.Domain/Interfaces/IProductRepository.cs`
- [X] T009 Create `ProductDbContext : DbContext` with `DbSet<Product>` in `src/Product.Infrastructure/Persistence/ProductDbContext.cs`
- [X] T010 Create `ProductConfiguration : IEntityTypeConfiguration<Product>` applying PK, unique index on Sku, `HasPrecision(18,2)` on Price, and `HasMaxLength` per data-model.md in `src/Product.Infrastructure/Persistence/Configurations/ProductConfiguration.cs`
- [X] T011 Implement `ProductRepository : IProductRepository` with EF Core using `ProductDbContext` in `src/Product.Infrastructure/Repositories/ProductRepository.cs`
- [X] T012 Create `ProductNotFoundException` and `DuplicateSkuException` domain exception classes in `src/Product.Application/Exceptions/ProductNotFoundException.cs` and `src/Product.Application/Exceptions/DuplicateSkuException.cs`
- [X] T013 Create `ProductResponse` DTO mirroring all 9 entity fields in `src/Product.Application/DTOs/ProductResponse.cs`
- [X] T014 Create `IProductService` interface skeleton (no methods yet) in `src/Product.Application/Interfaces/IProductService.cs`
- [X] T015 Create `ProductService` skeleton with constructor injection of `IProductRepository` in `src/Product.Application/Services/ProductService.cs`
- [X] T016 Create `AddInfrastructure(IServiceCollection, IConfiguration)` extension method registering `ProductDbContext` (Npgsql, `ConnectionStrings:ProductDb`) and `IProductRepository → ProductRepository` in `src/Product.Infrastructure/DependencyInjection.cs`
- [X] T017 Create `src/Product.Api/appsettings.Development.json` with `ConnectionStrings:ProductDb` pointing to local PostgreSQL per quickstart.md
- [X] T018 Create `ExceptionHandlingMiddleware` mapping `ProductNotFoundException → 404` and `DuplicateSkuException → 409` using `ProblemDetails` in `src/Product.Api/Middleware/ExceptionHandlingMiddleware.cs`
- [X] T019 Configure `src/Product.Api/Program.cs`: call `AddInfrastructure`, register `IProductService → ProductService`, add `AddProblemDetails`, add Swashbuckle, register `ExceptionHandlingMiddleware`, map controllers
- [X] T020 Create `ProductsController` skeleton with `[ApiController]`, `[Route("api/products")]`, `[Produces("application/json")]` and constructor injection of `IProductService` in `src/Product.Api/Controllers/ProductsController.cs`
- [X] T021 Start Docker Compose (`docker compose up -d`), then generate and apply the `InitialCreate` EF Core migration against `src/Product.Infrastructure` / `src/Product.Api`

**Checkpoint**: `dotnet build` passes. PostgreSQL container is running. Migration applied. Controller skeleton compiles with no endpoints yet.

---

## Phase 3: User Story 1 — Create a Product (Priority: P1) 🎯 MVP

**Goal**: A caller can POST a valid product and receive a 201 response with the new product ID. Duplicate SKUs and invalid payloads are rejected with structured errors.

**Independent Test**: `POST /api/products` with a valid payload → 201 + Location header. Re-POST with same SKU → 409. POST with empty name → 400 with `errors.Name`.

- [X] T022 [US1] Create `CreateProductRequest` DTO with DataAnnotations validation (`[Required]`, `[MaxLength]`, `[Range]`) in `src/Product.Application/DTOs/CreateProductRequest.cs`
- [X] T023 [US1] Add `Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken ct)` to `IProductService` in `src/Product.Application/Interfaces/IProductService.cs`
- [X] T024 [US1] Implement `ProductService.CreateAsync`: call `SkuExistsAsync` (throw `DuplicateSkuException` if true), map request to `Product` entity, set `Id = Guid.NewGuid()` and `CreatedAtUtc = DateTime.UtcNow`, call `AddAsync` + `SaveChangesAsync`, return mapped `ProductResponse` in `src/Product.Application/Services/ProductService.cs`
- [X] T025 [P] [US1] Implement `ProductsController.Create`: `[HttpPost]`, `[ProducesResponseType(typeof(ProductResponse), 201)]`, `[ProducesResponseType(typeof(ValidationProblemDetails), 400)]`, `[ProducesResponseType(typeof(ProblemDetails), 409)]`, call `_service.CreateAsync`, return `CreatedAtAction` in `src/Product.Api/Controllers/ProductsController.cs`
- [X] T026 [P] [US1] Write `ProductServiceCreateTests`: (a) valid request creates and returns product, (b) duplicate SKU throws `DuplicateSkuException`, (c) verify `CreatedAtUtc` is set to a UTC value in `tests/Product.UnitTests/Services/ProductServiceCreateTests.cs`

**Checkpoint**: `POST /api/products` works end-to-end. Swagger shows POST with 201/400/409 response types. User Story 1 independently testable.

---

## Phase 4: User Story 2 — Browse the Product Catalog (Priority: P2)

**Goal**: A caller can GET a paginated list of products with total count and page metadata.

**Independent Test**: Seed multiple products; `GET /api/products?pageNumber=1&pageSize=2` → returns 2 items and correct `totalCount`.

- [X] T027 [US2] Create `PagedResult<T>` generic DTO with `Items`, `TotalCount`, `PageNumber`, `PageSize` in `src/Product.Application/DTOs/PagedResult.cs`
- [X] T028 [US2] Add `Task<PagedResult<ProductResponse>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct)` to `IProductService` in `src/Product.Application/Interfaces/IProductService.cs`
- [X] T029 [US2] Implement `ProductService.GetPagedAsync`: call `repository.GetPagedAsync`, map each item to `ProductResponse`, return `PagedResult<ProductResponse>` in `src/Product.Application/Services/ProductService.cs`
- [X] T030 [US2] Implement `ProductsController.GetAll`: `[HttpGet]`, `pageNumber` (default 1, min 1) and `pageSize` (default 20, min 1, max 100) query params, `[ProducesResponseType(typeof(PagedResult<ProductResponse>), 200)]`, `[ProducesResponseType(400)]` in `src/Product.Api/Controllers/ProductsController.cs`
- [X] T031 [P] [US2] Write `ProductServiceGetPagedTests`: (a) non-empty catalog returns correct page, (b) empty catalog returns empty items with `totalCount = 0` in `tests/Product.UnitTests/Services/ProductServiceGetPagedTests.cs`

**Checkpoint**: `GET /api/products` works end-to-end. Swagger shows GET with 200/400. User Story 2 independently testable.

---

## Phase 5: User Story 3 — Retrieve a Single Product (Priority: P3)

**Goal**: A caller can fetch the full details of one product by its GUID identifier.

**Independent Test**: Create a product via US1 endpoint; `GET /api/products/{id}` → 200 with all fields. `GET /api/products/<unknown-guid>` → 404.

- [X] T032 [US3] Add `Task<ProductResponse> GetByIdAsync(Guid id, CancellationToken ct)` to `IProductService` in `src/Product.Application/Interfaces/IProductService.cs`
- [X] T033 [US3] Implement `ProductService.GetByIdAsync`: call `repository.GetByIdAsync`, throw `ProductNotFoundException` if null, map to `ProductResponse` in `src/Product.Application/Services/ProductService.cs`
- [X] T034 [US3] Implement `ProductsController.GetById`: `[HttpGet("{id:guid}")]`, `[ProducesResponseType(typeof(ProductResponse), 200)]`, `[ProducesResponseType(typeof(ProblemDetails), 404)]` in `src/Product.Api/Controllers/ProductsController.cs`
- [X] T035 [P] [US3] Write `ProductServiceGetByIdTests`: (a) existing id returns mapped response, (b) unknown id throws `ProductNotFoundException` in `tests/Product.UnitTests/Services/ProductServiceGetByIdTests.cs`

**Checkpoint**: `GET /api/products/{id}` works end-to-end. User Story 3 independently testable.

---

## Phase 6: User Story 4 — Update a Product (Priority: P4)

**Goal**: A caller can fully replace all fields of an existing product. Invalid data and unknown IDs are rejected.

**Independent Test**: Create a product; `PUT /api/products/{id}` with new name and price → 204. `GET /api/products/{id}` → returns updated values. PUT with unknown ID → 404.

- [X] T036 [US4] Create `UpdateProductRequest` DTO with DataAnnotations validation (same constraints as `CreateProductRequest` but all fields required) in `src/Product.Application/DTOs/UpdateProductRequest.cs`
- [X] T037 [US4] Add `Task UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct)` to `IProductService` in `src/Product.Application/Interfaces/IProductService.cs`
- [X] T038 [US4] Implement `ProductService.UpdateAsync`: fetch via `GetByIdAsync` (throws `ProductNotFoundException` if missing), copy all fields from request onto entity, set `UpdatedAtUtc = DateTime.UtcNow`, call `repository.Update` + `SaveChangesAsync` in `src/Product.Application/Services/ProductService.cs`
- [X] T039 [US4] Implement `ProductsController.Update`: `[HttpPut("{id:guid}")]`, `[ProducesResponseType(204)]`, `[ProducesResponseType(typeof(ValidationProblemDetails), 400)]`, `[ProducesResponseType(typeof(ProblemDetails), 404)]` in `src/Product.Api/Controllers/ProductsController.cs`
- [X] T040 [P] [US4] Write `ProductServiceUpdateTests`: (a) valid update mutates entity and sets `UpdatedAtUtc`, (b) unknown id throws `ProductNotFoundException` in `tests/Product.UnitTests/Services/ProductServiceUpdateTests.cs`

**Checkpoint**: `PUT /api/products/{id}` works end-to-end. User Story 4 independently testable.

---

## Phase 7: User Story 5 — Delete a Product (Priority: P5)

**Goal**: A caller can permanently remove a product by ID. Unknown IDs return 404.

**Independent Test**: Create a product; `DELETE /api/products/{id}` → 204. `GET /api/products/{id}` → 404. `DELETE /api/products/<unknown-guid>` → 404.

- [X] T041 [US5] Add `Task DeleteAsync(Guid id, CancellationToken ct)` to `IProductService` in `src/Product.Application/Interfaces/IProductService.cs`
- [X] T042 [US5] Implement `ProductService.DeleteAsync`: fetch via `GetByIdAsync` (throws `ProductNotFoundException` if missing), call `repository.Remove` + `SaveChangesAsync` in `src/Product.Application/Services/ProductService.cs`
- [X] T043 [US5] Implement `ProductsController.Delete`: `[HttpDelete("{id:guid}")]`, `[ProducesResponseType(204)]`, `[ProducesResponseType(typeof(ProblemDetails), 404)]` in `src/Product.Api/Controllers/ProductsController.cs`
- [X] T044 [P] [US5] Write `ProductServiceDeleteTests`: (a) existing product is removed and `SaveChangesAsync` is called, (b) unknown id throws `ProductNotFoundException` in `tests/Product.UnitTests/Services/ProductServiceDeleteTests.cs`

**Checkpoint**: `DELETE /api/products/{id}` works end-to-end. All 5 CRUD endpoints functional. User Story 5 independently testable.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, documentation annotations, and end-to-end smoke test.

- [X] T045 Add `[ProducesResponseType]` XML summary comments to all controller actions so Swagger UI shows operation descriptions in `src/Product.Api/Controllers/ProductsController.cs`
- [X] T046 [P] Run `dotnet build Ecommerce.sln` and confirm zero errors and zero warnings
- [X] T047 [P] Run `dotnet test tests/Product.UnitTests` and confirm all unit tests pass
- [X] T048 Run the quickstart.md end-to-end smoke test: `docker compose up -d` → apply migrations → `dotnet run --project src/Product.Api` → curl POST/GET/PUT/DELETE

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 — **BLOCKS all user story phases**
- **Phases 3–7 (User Stories)**: All depend on Phase 2 completion; can be worked sequentially (P1→P5) or in parallel by multiple developers
- **Phase 8 (Polish)**: Depends on all desired user stories being complete

### User Story Dependencies

| Story | Phase | Depends On | Notes |
|-------|-------|------------|-------|
| US1 — Create | Phase 3 | Phase 2 | Foundation for seeding test data in US2–US5 |
| US2 — List | Phase 4 | Phase 2 | Independent of US1 at service level |
| US3 — Get by ID | Phase 5 | Phase 2 | Independent; `GetByIdAsync` reused by US4/US5 |
| US4 — Update | Phase 6 | Phase 2 + T033 (GetByIdAsync) | Reuses `GetByIdAsync` from US3 |
| US5 — Delete | Phase 7 | Phase 2 + T033 (GetByIdAsync) | Reuses `GetByIdAsync` from US3 |

> **Note**: US4 and US5 both depend on `ProductService.GetByIdAsync` (T033) from US3. Complete US3 before starting US4 or US5, or extract `GetByIdAsync` into the Foundational phase if running stories in parallel.

### Within Each User Story

- DTO → Service interface method → Service implementation → Controller action
- Unit tests can be written in parallel with implementation (different files)

---

## Parallel Opportunities

### Phase 2 — Foundational

T007–T013 can all start in parallel (separate files), then T014–T020 layer on top.

### Phase 3 Example — User Story 1

```
Parallel launch:
  Task T022: CreateProductRequest DTO   (src/Product.Application/DTOs/CreateProductRequest.cs)
  Task T026: Unit test file skeleton     (tests/Product.UnitTests/Services/ProductServiceCreateTests.cs)

Sequential after T022:
  Task T023 → T024 → T025
```

### Phase 4 Example — User Story 2

```
Parallel launch:
  Task T027: PagedResult<T> DTO         (src/Product.Application/DTOs/PagedResult.cs)
  Task T031: Unit test file skeleton     (tests/Product.UnitTests/Services/ProductServiceGetPagedTests.cs)

Sequential after T027:
  Task T028 → T029 → T030
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001–T006)
2. Complete Phase 2: Foundational (T007–T021) — **critical path**
3. Complete Phase 3: User Story 1 (T022–T026)
4. **STOP and VALIDATE**: `POST /api/products` + 400/409 error cases
5. Demo working create endpoint before adding remaining stories

### Incremental Delivery

| Increment | Phases | Deliverable |
|-----------|--------|-------------|
| 1 (MVP) | 1 + 2 + 3 | Create product endpoint |
| 2 | + 4 | Paginated list endpoint |
| 3 | + 5 | Get-by-ID endpoint |
| 4 | + 6 | Update endpoint |
| 5 | + 7 | Delete endpoint |
| 6 | + 8 | Polish + full smoke test |

### Parallel Team Strategy (2 developers)

1. Both complete Phase 1 + Phase 2 together
2. Dev A: US1 (Phase 3) → US3 (Phase 5)
3. Dev B: US2 (Phase 4) → (wait for T033) → US4 (Phase 6) + US5 (Phase 7)
4. Both merge and run Phase 8

---

## Notes

- `[P]` tasks touch different files and have no incomplete-task dependencies — safe to run concurrently
- `[USn]` label maps every task to its spec.md user story for traceability
- No EF Core types (`DbContext`, `IQueryable`) may appear outside `Product.Infrastructure`
- `IProductService` must not be imported in `Product.Domain` — keep the dependency direction clean
- All timestamps assigned by the Application layer must use `DateTime.UtcNow`
- Commit after each Checkpoint for clean git history per story
