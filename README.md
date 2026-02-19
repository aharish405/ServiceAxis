# ServiceAxis — Enterprise Service Management Platform

> A production-ready, modular-monolith enterprise platform built with .NET 8, Clean Architecture, and SQL Server.

---

## 📋 Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Solution Structure](#solution-structure)
- [Technology Stack](#technology-stack)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [API Endpoints](#api-endpoints)
- [Authentication](#authentication)
- [Workflow Engine](#workflow-engine)
- [Background Jobs](#background-jobs)
- [Design Decisions](#design-decisions)
- [Roadmap](#roadmap)

---

## Overview

**ServiceAxis** is an enterprise service management platform designed for long-term evolution. Inspired by platforms like ServiceNow, it is architected as a **modular monolith** that can be cleanly extracted into microservices when scale demands it.

### Initial Modules

| Module | Status |
|--------|--------|
| Identity (Auth/RBAC) | ✅ Complete |
| Workflow Core Engine | ✅ Complete |
| Notification Abstraction | ✅ Placeholder |
| Audit Logging | ✅ Complete |
| ITSM / Incident Management | 🔜 Roadmap |
| Asset Management | 🔜 Roadmap |
| WorkAxis (HRMS) Integration | 🔜 Roadmap |

---

## Architecture

ServiceAxis follows **Clean Architecture** with a **Modular Monolith** pattern:

```
┌─────────────────────────────────────────────────────┐
│                   ServiceAxis.API                    │
│         (Controllers, Middleware, Swagger)            │
└──────────────────────┬──────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────┐
│              ServiceAxis.Application                 │
│    (Use Cases, CQRS Handlers, DTOs, Interfaces)      │
└──────┬──────────────────────────────┬───────────────┘
       │                              │
┌──────▼──────┐              ┌────────▼──────────────┐
│ServiceAxis  │              │  ServiceAxis.Identity  │
│  .Domain    │              │  (JWT Auth, RBAC)      │
│(Entities,   │              └────────────────────────┘
│ Enums,      │
│ DomainEvents│              ┌────────────────────────┐
└─────────────┘              │ServiceAxis.Infrastructure
                             │(EF Core, Repositories, │
                             │ Hangfire, Caching)     │
                             └────────────────────────┘
```

### Dependency Rule
- **Domain** → No external dependencies
- **Application** → Depends only on Domain + Shared
- **Infrastructure** → Implements Application contracts
- **Identity** → Implements Application auth contracts
- **API** → Composes everything; depends on all layers

---

## Solution Structure

```
ServiceAxis/
├── src/
│   ├── ServiceAxis.API/                # HTTP entry point
│   │   ├── Controllers/               # API controllers
│   │   ├── Middleware/                # Global exception + request logging
│   │   └── Program.cs                 # Composition root
│   │
│   ├── ServiceAxis.Application/       # Business use cases
│   │   ├── Behaviours/               # MediatR pipeline (Validation, Logging)
│   │   ├── Contracts/                # Interfaces for Infrastructure/Identity
│   │   │   ├── Persistence/
│   │   │   ├── Infrastructure/
│   │   │   └── Identity/
│   │   └── Features/                 # CQRS Commands & Queries by feature
│   │       └── Workflow/
│   │
│   ├── ServiceAxis.Domain/            # Enterprise domain model
│   │   ├── Common/                   # BaseEntity, AggregateRoot, PagedResult
│   │   ├── Entities/                 # Tenant, ApplicationUser, AuditLog
│   │   │   └── Workflow/             # WorkflowDefinition, Instance, Step, etc.
│   │   └── Enums/                    # Platform-wide enumerations
│   │
│   ├── ServiceAxis.Infrastructure/    # Data access & external services
│   │   ├── BackgroundJobs/           # Hangfire job definitions
│   │   ├── Persistence/
│   │   │   ├── Configurations/       # EF Core entity type configurations
│   │   │   ├── Migrations/           # EF Core migrations
│   │   │   ├── Repositories/         # Generic repository + Unit of Work
│   │   │   ├── DbSeeder.cs           # Seed data (roles, admin user)
│   │   │   └── ServiceAxisDbContext  # Main DbContext
│   │   └── Services/                 # Cache, Email, SMS implementations
│   │
│   ├── ServiceAxis.Identity/          # Auth & JWT token management
│   │   └── Services/                 # AuthService, CurrentUserService
│   │
│   ├── ServiceAxis.Workflow/          # Workflow engine (extensible)
│   ├── ServiceAxis.Shared/            # Cross-cutting utilities
│   │   ├── Exceptions/               # Domain exception hierarchy
│   │   ├── Settings/                 # Strongly-typed settings classes
│   │   └── Wrappers/                 # ApiResponse<T> envelope
│   └── ServiceAxis.Modules/           # Feature modules placeholder
│
└── tests/
    └── ServiceAxis.UnitTests/         # xUnit tests
```

---

## Technology Stack

| Concern | Technology |
|---------|-----------|
| Runtime | .NET 8 / ASP.NET Core |
| Database | Microsoft SQL Server |
| ORM | Entity Framework Core 8 |
| Mediator | MediatR 12 |
| Validation | FluentValidation 11 |
| Authentication | ASP.NET Identity + JWT Bearer |
| API Docs | Swagger / OpenAPI 3 |
| Logging | Serilog (Console + Rolling File) |
| Background Jobs | Hangfire (SQL Server storage) |
| Caching | In-Memory (Redis-ready interface) |
| Unit Tests | xUnit + Moq + FluentAssertions |

---

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (2019+) or SQL Server Express / LocalDB
- `dotnet-ef` tools: `dotnet tool install --global dotnet-ef`

### 1. Clone & Configure

Edit `src/ServiceAxis.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=tcp:YOUR_SERVER;Initial Catalog=ServiceAxis;User ID=sa;Password=YOUR_PASSWORD;Encrypt=False"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyMin32CharactersLong!"
  }
}
```

### 2. Run Migrations

```bash
# From the solution root
dotnet ef database update \
  --project src/ServiceAxis.Infrastructure \
  --startup-project src/ServiceAxis.API
```

### 3. Run the API

```bash
dotnet run --project src/ServiceAxis.API
```

The Swagger UI opens automatically at **http://localhost:5170**

### 4. Default Admin Credentials

| Field | Value |
|-------|-------|
| Email | `admin@serviceaxis.io` |
| Password | `Admin@123!` |
| Roles | `SuperAdmin`, `Admin` |

> ⚠️ Change these credentials immediately in any non-development environment.

---

## Configuration

### `JwtSettings`

| Key | Description | Default |
|-----|-------------|---------|
| `Issuer` | Token issuer | `ServiceAxis` |
| `Audience` | Token audience | `ServiceAxis.Clients` |
| `SecretKey` | HMAC-SHA256 signing key (min 32 chars) | — |
| `ExpiryMinutes` | Access token lifetime | `60` |
| `RefreshTokenExpiryDays` | Refresh token lifetime | `7` |

### `AppSettings`

| Key | Description |
|-----|-------------|
| `ApplicationName` | Platform display name |
| `BaseUrl` | API base URL for link generation |
| `ShowDetailedErrors` | Expose exception details in API responses |

---

## API Endpoints

### Auth
| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/Auth/login` | Login → JWT + refresh token |
| POST | `/api/Auth/register` | Register new user |
| POST | `/api/Auth/refresh` | Refresh access token |
| POST | `/api/Auth/revoke` | Revoke refresh token |

### Platform (secured)
| Method | Route | Auth Required |
|--------|-------|---------------|
| GET | `/api/Platform/me` | Any authenticated user |
| GET | `/api/Platform/admin-dashboard` | `Admin`, `SuperAdmin` only |

### Workflow Definitions (secured)
| Method | Route | Auth Required |
|--------|-------|---------------|
| GET | `/api/WorkflowDefinitions` | Role: Agent+ |
| POST | `/api/WorkflowDefinitions` | Role: Manager+ |

### Other
| Route | Description |
|-------|-------------|
| `/health` | Platform health check (anonymous) |
| `/hangfire` | Hangfire dashboard (dev only) |
| `/swagger` | API documentation (dev only) |

---

## Authentication

ServiceAxis uses **JWT Bearer tokens** with role-based and policy-based authorization.

### Authorization Policies

| Policy | Roles Allowed |
|--------|---------------|
| `AdminOnly` | SuperAdmin, Admin |
| `ManagerUp` | SuperAdmin, Admin, Manager |
| `AgentUp` | SuperAdmin, Admin, Manager, Agent |
| `AnyAuthenticated` | Any authenticated user |

### Usage in Swagger

1. Call `POST /api/Auth/login` to get an `accessToken`
2. Click **Authorize** (top right in Swagger UI)
3. Enter `Bearer <your-access-token>`
4. All secured endpoints will now work

---

## Workflow Engine

The workflow engine is built on **state-machine concepts**:

```
WorkflowDefinition  (blueprint/template)
       │
       ├── WorkflowSteps   (nodes: states)
       │       └── StepType: Manual | Approval | Automated | Notification
       │
       └── WorkflowTransitions  (edges: event-driven state changes)
               └── TriggerEvent: "Approved" | "Rejected" | "Escalated" ...
       
WorkflowInstance    (live execution)
       └── WorkflowActions  (full audit trail of every step action)
```

### Example: Create an ITSM Incident Approval Workflow

```http
POST /api/WorkflowDefinitions
Authorization: Bearer <token>

{
  "code": "INCIDENT_APPROVAL",
  "name": "Incident Approval Workflow",
  "description": "Standard IT incident review and approval process",
  "category": "ITSM"
}
```

---

## Background Jobs

Hangfire processes are registered automatically at startup:

| Job ID | Schedule | Description |
|--------|----------|-------------|
| `platform-health-check` | Every minute | Pings platform subsystems |
| `audit-log-cleanup` | Daily | Purges old soft-deleted audit records |

**Dashboard:** http://localhost:5170/hangfire (development only)

---

## Design Decisions

### Why Modular Monolith?
ServiceAxis starts as a monolith for simplicity and operational efficiency, but each `src/` project maps to a future microservice boundary. The domain/application separation ensures extraction is clean when scale requires it.

### Why MediatR + CQRS?
Commands and queries are separated from day one. This prevents fat controllers, enables easy testing, and the pipeline behaviour system (validation, logging) is a cross-cutting concern that scales elegantly.

### Why Generic Repository + Unit of Work?
The patterns decouple business logic from EF Core specifics, making the application layer testable with mocks without needing an in-memory database.

### Why Schema Separation?
- `[identity]` — ASP.NET Identity tables
- `[platform]` — Core tenant, user, and audit tables
- `[workflow]` — Workflow engine tables

This enables clean data governance and will make future database sharding or per-module schema isolation simpler.

### Why In-Memory Cache (not Redis yet)?
The `ICacheService` abstraction is production-ready. Swap `MemoryCacheService` for a Redis implementation in `Infrastructure/DependencyInjection.cs` without touching any business logic.

---

## Roadmap

- [ ] ITSM Module (Incidents, Service Requests, SLAs)
- [ ] Asset Management Module
- [ ] Notification delivery (SendGrid / Azure Communication Services)
- [ ] Redis cache implementation
- [ ] WorkAxis (HRMS) integration hooks
- [ ] Multi-tenancy enforcement middleware
- [ ] OpenTelemetry / distributed tracing
- [ ] Docker + docker-compose support
- [ ] Azure AD / OIDC integration

---

*Built with ❤️ using Clean Architecture principles. Designed for the long-term.*
