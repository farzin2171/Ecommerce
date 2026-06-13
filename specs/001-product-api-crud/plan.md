# Implementation Plan: Product API — CRUD Microservice

**Branch**: `001-product-api-crud` | **Date**: 2026-06-12 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `specs/001-product-api-crud/spec.md`

## Summary

Build a REST API microservice that exposes full CRUD operations for a single `Product` entity backed by PostgreSQL. The service follows a strict layered architecture (API → Application → Domain ← Infrastructure) with the repository pattern. No authentication, no cross-service communication, no CQRS — intentionally simple as a first microservice in the e-commerce platform.

## Technical Context

**Language/Version**: C# / .NET 10 (ASP.NET Core Web API)  
**Primary Dependencies**: ASP.NET Core (controller-based), EF Core 9, Npgsql.EntityFrameworkCore.PostgreSQL, Swashbuckle.AspNetCore  
**Storage**: PostgreSQL 16 (containerised locally via Docker Compose)  
**Testing**: xUnit + EF Core InMemory or Testcontainers-dotnet (unit tests for Application layer)  
**Target Platform**: Linux/Windows server; local dev on Docker Desktop  
**Project Type**: web-service (REST microservice)  
**Performance Goals**: Standard CRUD latency; no high-throughput requirements for this iteration  
**Constraints**: No auth, no CQRS, no Minimal API, no cross-service calls; single entity  
**Scale/Scope**: Local dev; single microservice; one entity; five endpoints

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

The project constitution is currently a placeholder with no ratified principles. No governance violations to evaluate. All gates pass by default.

**Post-Phase 1 re-check**: No new violations introduced. The four-project solution structure is the minimum required by the layered architecture mandate in the technical spec.

## Project Structure

### Documentation (this feature)

```text
specs/001-product-api-crud/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
│   └── products-api.md
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)

```text
src/
├── Product.Api/                  # Web API: controllers, Program.cs, appsettings.json, middleware
├── Product.Application/          # Service layer: IProductService, ProductService, DTOs, validators
├── Product.Domain/               # Entities: Product, IProductRepository
└── Product.Infrastructure/       # EF Core: ProductDbContext, ProductRepository, migrations

tests/
└── Product.UnitTests/            # xUnit: Application layer unit tests

docker-compose.yml                # PostgreSQL 16 for local dev
Ecommerce.sln
```

**Structure Decision**: Multi-project .NET solution. Four `src/` projects enforce the dependency direction (`Api → Application → Domain ← Infrastructure`) required by the layered architecture. One test project covers the Application service layer where business logic lives.
