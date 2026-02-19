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
| Audit Logging | ✅ Complete |
| Activity Stream / Collaboration | ✅ Complete |
| Multi-Tenancy (Data Isolation) | ✅ Complete |
| Granular RBAC (Table/Field) | ✅ Complete |
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
| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| POST | `/api/Auth/login` | Login → JWT + refresh token | Public |
| POST | `/api/Auth/register` | Register new user | Public |
| POST | `/api/Auth/refresh` | Refresh access token | Public |
| POST | `/api/Auth/revoke` | Revoke refresh token | Authenticated |

### Platform
| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/api/Platform/me` | Current user context | Any |
| GET | `/api/Platform/permissions` | User's granted permissions | Any |
| GET | `/api/Platform/stats` | Live platform statistics | AgentUp |
| GET | `/api/Platform/admin-dashboard` | Admin dashboard with stats | AdminOnly |

### Records (Dynamic Table Data)
| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| POST | `/api/v1/records/{table}` | Create a record | Any |
| GET | `/api/v1/records/{table}` | List records (with filters) | Any |
| GET | `/api/v1/records/{table}/{id}` | Get record by ID | Any |
| PUT | `/api/v1/records/{table}/{id}` | Update a record | Any |
| DELETE | `/api/v1/records/{table}/{id}` | Soft-delete a record | ManagerUp |
| POST | `/api/v1/records/{table}/{id}/assign` | Assign record to user/group | AgentUp |
| GET | `/api/v1/records/{table}/{id}/activities` | Activity timeline | Any |
| POST | `/api/v1/records/{table}/{id}/comments` | Add a comment | Any |
| POST | `/api/v1/records/{table}/{id}/attachments` | Upload attachment | Any |

### Metadata (Schema Management)
| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/api/v1/metadata/tables` | List all registered tables | AgentUp |
| GET | `/api/v1/metadata/tables/{name}` | Get table + field schema | AgentUp |
| POST | `/api/v1/metadata/tables` | Register a new table | AdminOnly |
| POST | `/api/v1/metadata/tables/{name}/fields` | Add a field to a table | AdminOnly |
| GET | `/api/v1/metadata/forms/{table}` | Get form schema for table | AgentUp |

### Workflow Definitions
| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/api/WorkflowDefinitions` | List workflow definitions | AgentUp |
| POST | `/api/WorkflowDefinitions` | Create new definition (draft) | ManagerUp |
| GET | `/api/WorkflowDefinitions/{id}` | Get definition + full graph | AgentUp |
| GET | `/api/WorkflowDefinitions/{id}/steps` | List steps | AgentUp |
| POST | `/api/WorkflowDefinitions/{id}/steps` | Add step | ManagerUp |
| PUT | `/api/WorkflowDefinitions/{id}/steps/{stepId}` | Update step | ManagerUp |
| DELETE | `/api/WorkflowDefinitions/{id}/steps/{stepId}` | Delete step | AdminOnly |
| GET | `/api/WorkflowDefinitions/{id}/transitions` | List transitions | AgentUp |
| POST | `/api/WorkflowDefinitions/{id}/transitions` | Add transition | ManagerUp |
| DELETE | `/api/WorkflowDefinitions/{id}/transitions/{id}` | Delete transition | AdminOnly |
| GET | `/api/WorkflowDefinitions/{id}/instances` | List running instances | AgentUp |

### SLA Management
| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/api/Sla` | List SLA definitions | AgentUp |
| GET | `/api/Sla/{id}` | Get SLA definition + policies | AgentUp |
| POST | `/api/Sla` | Create SLA definition | AdminOnly |
| PUT | `/api/Sla/{id}` | Update SLA definition | AdminOnly |
| POST | `/api/Sla/{id}/policies` | Add priority-tier policy | AdminOnly |

### Notification Templates
| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/api/NotificationTemplates` | List templates | AgentUp |
| GET | `/api/NotificationTemplates/{id}` | Get template with body | AgentUp |
| POST | `/api/NotificationTemplates` | Create template | AdminOnly |
| PUT | `/api/NotificationTemplates/{id}` | Update template | AdminOnly |
| DELETE | `/api/NotificationTemplates/{id}` | Delete template | AdminOnly |

### Admin: Tenants
| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/api/v1/Admin/tenants` | List tenants | AdminOnly |
| GET | `/api/v1/Admin/tenants/{id}` | Get tenant | AdminOnly |
| POST | `/api/v1/Admin/tenants` | Create tenant | AdminOnly |
| PUT | `/api/v1/Admin/tenants/{id}` | Update tenant | AdminOnly |
| DELETE | `/api/v1/Admin/tenants/{id}` | Deactivate tenant | AdminOnly |

### Admin: Users
| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/api/v1/Admin/users` | List users | AdminOnly |
| GET | `/api/v1/Admin/users/{id}` | Get user detail | AdminOnly |
| POST | `/api/v1/Admin/users/{id}/roles` | Assign role | AdminOnly |
| DELETE | `/api/v1/Admin/users/{id}/roles/{role}` | Remove role | AdminOnly |

### Assignment Groups
| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/api/AssignmentGroups` | List groups with member counts | AgentUp |
| GET | `/api/AssignmentGroups/{id}` | Get group + all members | AgentUp |
| POST | `/api/AssignmentGroups` | Create group | AdminOnly |
| PUT | `/api/AssignmentGroups/{id}` | Update group settings | AdminOnly |
| POST | `/api/AssignmentGroups/{id}/members` | Add member | ManagerUp |
| DELETE | `/api/AssignmentGroups/{id}/members/{memberId}` | Remove member | ManagerUp |

### Audit Logs
| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/api/Audit` | Query audit trail (filtered + paged) | ManagerUp |

### Infrastructure
| Route | Description | Auth |
|-------|-------------|------|
| `GET /health` | Health check | Public |
| `/hangfire` | Hangfire dashboard | Dev only |
| `/swagger` | API documentation | Dev only |

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

### Multi-Tenancy Isolation

ServiceAxis implements **Horizontal Data Isolation** using EF Core **Global Query Filters**. 
- Every query is transparently filtered by the `TenantId` of the authenticated user.
- Metadata (Tables, Fields, Forms) is also tenant-aware via the `MetadataCache`.
- Cross-tenant data leakage is prevented at the database driver level.

### Granular RBAC (Table & Field Level)

The platform provides a highly flexible security model beyond just roles:
- **Table Permissions:** Control `Create`, `Read`, `Write`, `Delete`, and `Admin` access per table (e.g., "incident", "change_request").
- **Field Permissions:** Control `Read` and `Write` visibility for individual fields (e.g., "priority", "calculated_cost").
- **Internal Note Privacy:** Activity timeline entries tagged as `IsInternal` are hidden from users without the `platform.activity.internal_notes` permission.
- **Permission Service:** Unified `IPermissionService` for checking accessibility in business logic and UI.

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

### Event-Driven Triggers

Workflows can be triggered automatically based on record mutations:
- **RecordCreated:** Fire a flow when a new record appears.
- **RecordUpdated / FieldChanged:** Fire a flow when specific values change (e.g., Priority becomes "Critical").
- **Activity Integration:** Every workflow start/milestone is automatically logged to the record's **Activity Stream** (Timeline), providing visibility into automated platform actions.

---

## Universal Activity Stream

ServiceAxis features a unified collaboration and audit timeline shared across **every module**. No module-specific logging code is needed — activity is generated automatically by the platform's record engine, workflow engine, and attachment system.

### Architecture

```
PlatformRecord
    └── Activity[]                   ← ordered by CreatedAt DESC
            ├── FieldChange[]        ← field-level diff (old/new values)
            ├── Comment[]            ← user comments + work notes
            └── (AttachmentAdded)    ← links to Attachment entity
```

### Event Types

| ActivityType | Trigger |
|-------------|---------|
| `RecordCreated` | New record saved (auto) |
| `FieldChanged` | One or more field values changed (auto, with full diff) |
| `StatusChanged` | The `state` field specifically changed (auto) |
| `CommentAdded` | User posted a public comment |
| `WorkNoteAdded` | Agent posted an internal note |
| `AttachmentAdded` | File uploaded (auto) |
| `WorkflowEvent` | Workflow trigger fired (auto from WorkflowEngine) |

### All event generation is centralised — zero controller-level logging

```
CreateRecordHandler  ──► IActivityService.LogActivityAsync(RecordCreated)
UpdateRecordHandler  ──► IActivityService.LogActivityAsync(FieldChanged/StatusChanged, changes[])
WorkflowEngine       ──► IActivityService.LogActivityAsync(WorkflowEvent)
AddAttachmentHandler ──► IActivityService.LogActivityAsync(AttachmentAdded)
AddCommentHandler    ──► IActivityService.AddCommentAsync(isInternal)
```

### API

```http
# Fetch the activity timeline for a record
GET /api/v1/records/{table}/{id}/activities?page=1&pageSize=20
Authorization: Bearer <token>
```

Sample response:
```json
{
  "items": [
    {
      "type": "FieldChanged",
      "createdBy": "admin@corp.com",
      "createdAt": "2026-02-19T10:00:00Z",
      "isSystem": true,
      "changes": [
        { "field": "priority", "old": "3", "new": "1" }
      ]
    },
    {
      "type": "CommentAdded",
      "createdBy": "jsmith@corp.com",
      "createdAt": "2026-02-19T09:55:00Z",
      "isSystem": false,
      "comment": { "text": "Investigating now.", "isInternal": false }
    }
  ],
  "totalCount": 12,
  "pageNumber": 1,
  "pageSize": 20
}
```

```http
# Add a comment
POST /api/v1/records/{table}/{id}/comments
{ "text": "Escalating to L2.", "isInternal": false }

# Add a work note (internal only)
POST /api/v1/records/{table}/{id}/comments
{ "text": "Called vendor, awaiting callback.", "isInternal": true }

# Upload an attachment
POST /api/v1/records/{table}/{id}/attachments    (multipart/form-data)

# Download an attachment
GET /api/v1/attachments/{attachmentId}

# Delete an attachment
DELETE /api/v1/attachments/{attachmentId}
```

### Security
- **Work Notes** (IsInternal=true) are filtered from non-privileged users. Requires the `platform.activity.internal_notes` permission.
- Record access is gate-kept by the existing RBAC table/field permission model — users cannot read activities for records they cannot access.

### Performance
- The `Activity` table is indexed on `(RecordId, CreatedAt)` for O(log n) timeline queries.
- All timeline queries are paginated — no unbounded result sets.
- Attachment binary content is never loaded as part of activity queries.

### Storage Extensibility
The `IFileStorageProvider` abstraction lets you swap the storage back-end with zero application-layer changes:

| Implementation | Swap in `DependencyInjection.cs` | Status |
|---------------|----------------------------------|--------|
| `LocalFileStorageProvider` | Default | ✅ Implemented |
| `AzureBlobStorageProvider` | Replace `AddScoped<IFileStorageProvider, ...>` | 🔜 Ready to implement |
| `S3StorageProvider` | Replace `AddScoped<IFileStorageProvider, ...>` | 🔜 Ready to implement |

---

## Assignment & Lifecycle Management

The platform includes a robust engine for managing work distribution and process flow, fully configurable via metadata.

### Assignment System
- **Assignment Groups:** Organize agents into functional teams (e.g., "Service Desk", "Network Ops").
- **Queues & Routing:** 
  - Define queues (e.g., "Critical Incidents") based on table and priority.
  - **Auto-Assignment Strategies:** Round-Robin, Least Loaded, or Manual.
- **Unified Model:** Records can be assigned to a User, a Group, or both.
- **Audit:** All assignment changes are logged to the Activity Stream automatically.

### State Machine Engine
Every table can define its own lifecycle states and transitions:
- **States:** Define stages like `New`, `In Progress`, `Pending`, `Resolved`, `Closed`.
- **Transitions:** Control the allowed path (e.g., `New` → `In Progress` is allowed, but `New` → `Resolved` is forbidden).
- **RBAC:** Restrict specific transitions to certain roles (e.g., only `Manager` can `Close` a record).
- **Guardrails:** The engine validates every state change against the defined rules before committing.

### Activity Integration
All lifecycle events generate standard activity types:
- `AssignmentChanged` — when user or group changes.
- `StateTransitioned` — when state moves (e.g., New → In Progress).

### API Usage

#### Get Available Transitions for a Record
```http
GET /api/v1/records/incident/{id}/transitions
Authorization: Bearer <token>
```
Response:
```json
[
  { "id": "uuid", "toState": "resolved", "label": "Resolve Incident" },
  { "id": "uuid", "toState": "pending", "label": "Hold" }
]
```

#### Execute Transition
```http
POST /api/v1/records/incident/{id}/state
{
  "targetStateId": "uuid-of-resolved-state",
  "comment": "Fix applied."
}
```

#### Define an Assignment Group (Admin)
```http
POST /api/v1/assignment-groups
{
  "name": "Service Desk",
  "email": "help@serviceaxis.io",
  "defaultStrategy": "RoundRobin"
}
```

---

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

- [x] Multi-tenancy enforcement (Query Filters + TenantId isolation)
- [x] Granular RBAC (Table + Field level permissions)
- [x] Activity Stream (timeline, comments, attachments)
- [x] Workflow Engine (definitions, steps, transitions, triggers, instances)
- [x] SLA Engine (definitions, priority policies, instance tracking)
- [x] Dynamic Record API (schema-driven full CRUD)
- [x] Metadata Admin API (table/field management)
- [x] Notification Templates (create/update/delete with system-template protection)
- [x] Form Engine (JSON schema for frontend auto-rendering)
- [x] Assignment Engine (auto-assign + manual assign with round-robin/least-loaded)
- [x] Platform Dashboard Stats API
- [x] Admin Controllers (Tenant + User management)
- [ ] ITSM Module UI (Incidents → Service Requests → Change Requests)
- [ ] Asset Management Module
- [ ] Notification delivery (SendGrid / Azure Communication Services)
- [ ] Redis cache implementation
- [ ] WorkAxis (HRMS) integration hooks
- [ ] OpenTelemetry / distributed tracing
- [ ] Docker + docker-compose support
- [ ] Azure AD / OIDC integration
- [ ] Frontend (React/Vue) with form engine integration
- [ ] Unit + Integration test suite

---

*Built with ❤️ using Clean Architecture principles. Designed for long-term evolution.*
