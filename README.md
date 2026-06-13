# RestoPulse 🍽️

> A production-grade Restaurant Point-of-Sale and Order Management System built with **.NET 10 Microservices**, **.NET Aspire**, and a **Vanilla JS frontend**.

---

## Overview

RestoPulse is a full-stack restaurant management platform designed to handle the complete lifecycle of a dine-in restaurant operation — from table management and order taking to kitchen display, billing, inventory tracking, and revenue reporting.

The project is built to industry standards, demonstrating real-world microservice architecture patterns using the latest .NET 10 ecosystem. It serves as both a functional POS system and a reference architecture for building distributed systems with .NET Aspire.

---

## Live Features

| Module | Description |
|---|---|
| 🪑 **Table Management** | Floor plan view, table status, section management, real-time occupancy |
| 🧾 **Order Management** | Full order lifecycle — New → Preparing → Served → Billed, item-level control |
| 🍳 **Kitchen Display (KDS)** | Live ticket queue, urgency timers, bump screen, category filters |
| 💳 **Billing & Invoices** | GST-compliant invoices, split bill, Cash/Card/UPI settlement |
| 📋 **Menu Manager** | Categories, items, pricing, availability toggle |
| 📦 **Inventory** | Stock levels, low-stock alerts, manual adjustments |
| 📊 **Reports** | Daily revenue, top-selling items, monthly inventory consumption, order analytics |
| 👥 **Users & Shifts** | Staff profiles, role permissions (Owner/Manager/Chef/Server), shift scheduler, clock-in/out tracking (with late detection), and monthly hours reports |

---

## Architecture

RestoPulse follows a **microservice architecture** with each business domain owned by an independent service with its own database. Services communicate asynchronously via **RabbitMQ** using integration events.

```
┌─────────────────────────────────────────────────────────────┐
│                      Frontend (Vanilla JS)                  │
│                RestoPulse UI — index.html                   │
└─────────────────────────────┬───────────────────────────────┘
                              │ HTTP
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     GatewayService (YARP)                   │
│                JWT Auth · Rate Limiting · Routing           │
└──┬──────┬──────┬──────┬──────┬──────┬──────┬──────┬─────────┘
   │      │      │      │      │      │      │      │
   ▼      ▼      ▼      ▼      ▼      ▼      ▼      ▼
 Menu  Table  Order  Kitchen Billing Inventory Report  User
  Svc   Svc    Svc    Svc     Svc     Svc      Svc     Svc
   │      │      │      │       │       │        │      │
   ▼      ▼      ▼      ▼       ▼       ▼        ▼      ▼
 menudb tabledb orderdb kitchendb billingdb invdb reportdb userdb
                    │                              ▲
                    └──── RabbitMQ Events ─────────┘
```

### Event Flow

```
POST /api/orders
  → OrderService saves order
  → Publishes OrderCreated event

KitchenService (subscriber)
  → Receives OrderCreated
  → Creates kitchen tickets per item
  → KDS displays them in real-time

InventoryService (subscriber)
  → Receives OrderStatusChanged (Served)
  → Auto-deducts ingredients from stock
  → Publishes LowStockAlert if threshold breached

ReportService (subscriber)
  → Receives all events
  → Builds materialized read models
  → Powers dashboard analytics
```

---

## Tech Stack

### Backend
| Concern | Technology |
|---|---|
| Framework | ASP.NET Core 10 Minimal APIs |
| Orchestration | .NET Aspire 13 |
| ORM | Entity Framework Core 10 + SQL Server |
| CQRS / Mediator | MediatR |
| Validation | FluentValidation |
| Messaging | MassTransit + RabbitMQ |
| API Gateway | YARP (Yet Another Reverse Proxy) |
| API Docs | Scalar UI + OpenAPI |
| Observability | OpenTelemetry → Aspire Dashboard |
| Architecture | Clean Architecture + DDD |

### Frontend
| Concern | Technology |
|---|---|
| UI | Vanilla JS + HTML5 + CSS3 |
| Design System | Custom dark-mode design tokens |
| Routing | Client-side SPA router |
| API Client | Fetch API with centralized error handling |
| Charts | Pure CSS/JS bar charts |

### Infrastructure
| Concern | Technology |
|---|---|
| Database | SQL Server (containerized via Aspire) |
| Message Broker | RabbitMQ (containerized via Aspire) |
| Container Runtime | Docker Desktop |

---

## Project Structure

```
RestoPulse/
├── AppHost/                          # .NET Aspire orchestrator
│   └── RestoPulse.AppHost/
├── ServiceDefaults/                  # Shared Aspire defaults
│   └── RestoPulse.ServiceDefaults/   # OpenTelemetry, health checks, resilience
├── Services/
│   ├── MenuService/                  # Menu categories + items CRUD
│   ├── TableService/                 # Tables, sections, status + events
│   ├── OrderService/                 # Orders, items, lifecycle + events
│   ├── KitchenService/               # KDS tickets, subscribes to OrderCreated
│   ├── BillingService/               # Bills, GST, payments
│   ├── InventoryService/             # Stock, adjustments, auto-deduction
│   ├── ReportService/                # Revenue aggregations, read models
│   ├── UserService/                  # Staff management, shifts & schedules
│   └── GatewayService/               # YARP reverse proxy, JWT auth, hosts SPA frontend under wwwroot/
└── Shared/
    ├── RestoPulse.Contracts/         # Shared DTOs and integration events
    └── RestoPulse.SharedKernel/      # Base classes, Result pattern
```

The SPA frontend structure inside `Services/GatewayService/wwwroot/` is:
```
wwwroot/
├── index.html                        # App shell + sidebar
├── css/main.css                      # Design system
├── components/
│   ├── js/app.js                     # Router, API client, helpers, global badge poller
│   └── modules/                      # One JS file per domain module
│       ├── dashboard.js
│       ├── tables.js
│       ├── orders.js
│       ├── kitchen.js
│       ├── billing.js
│       ├── menu-inventory-reports.js
│       └── users.js
```

Each service follows the Clean Architecture internal structure:
```
ServiceName/
├── Api/Endpoints/        # Minimal API route handlers
├── Application/
│   ├── Commands/         # Write operations (MediatR IRequest)
│   └── Queries/          # Read operations (MediatR IRequest)
├── Domain/
│   ├── Entities/         # Aggregate roots with private setters
│   ├── Enums/
│   └── Events/           # Domain + integration events
├── Infrastructure/
│   ├── Persistence/      # EF Core DbContext + Migrations + seeders
│   └── Messaging/        # MassTransit consumers
└── Contracts/            # Request/Response DTOs
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [JetBrains Rider](https://www.jetbrains.com/rider/)

### Setup

**1. Clone the repository**
```bash
git clone https://github.com/yourusername/RestoPulse.git
cd RestoPulse
```

**2. Install Aspire templates (first time only)**
```bash
dotnet new install Aspire.ProjectTemplates
```

**3. Set SQL Server password**

Create `AppHost/appsettings.Development.json`:
```json
{
  "Parameters": {
    "sql-password": "YourPassword@123!"
  }
}
```

**4. Run**
```bash
cd AppHost
dotnet run
```

The Aspire dashboard opens at `https://localhost:15000`.
All services, databases, and RabbitMQ start automatically via Docker.

**5. Open the frontend**

The frontend is served directly by the `GatewayService` when the application runs. You can access it at `https://localhost:7055` (or whichever GatewayService port is assigned by the Aspire dashboard).

Alternatively, you can open `Services/GatewayService/wwwroot/index.html` directly in a browser. The frontend runs fully on mock data out of the box — no backend required to explore the UI.

To connect to the live backend when running standalone, update `API.BASE` in `Services/GatewayService/wwwroot/components/js/app.js`:
```js
BASE: 'https://localhost:7055/api'  // GatewayService URL from Aspire dashboard
```

---

## API Documentation

Each service exposes a Scalar UI at `/scalar/v1`. Find the service URLs in the Aspire dashboard then navigate to:

| Service | Scalar UI |
|---|---|
| MenuService | `https://localhost:{port}/scalar/v1` |
| TableService | `https://localhost:{port}/scalar/v1` |
| OrderService | `https://localhost:{port}/scalar/v1` |
| KitchenService | `https://localhost:{port}/scalar/v1` |
| BillingService | `https://localhost:{port}/scalar/v1` |
| InventoryService | `https://localhost:{port}/scalar/v1` |
| ReportService | `https://localhost:{port}/scalar/v1` |
| UserService | `https://localhost:{port}/scalar/v1` |

Ports are assigned dynamically by Aspire and shown in the dashboard.

---

## Key Design Decisions

**One database per service** — true data isolation. No cross-service joins. Each service owns its schema completely.

**Private setters on domain entities** — domain logic lives in the entity, not in handlers. Entities can only be mutated through explicit methods like `order.SetStatus()` or `table.SetStatus()`.

**MediatR for CQRS** — commands and queries are fully separated. Handlers are single-responsibility and easily unit testable.

**Integration events via MassTransit** — services never call each other directly over HTTP for writes. State changes are communicated through the bus, keeping services loosely coupled.

**Auto-migrate on startup (dev only)** — `db.Database.MigrateAsync()` runs in development so the database schema is always in sync without manual steps.

---

## Roadmap

- [x] GatewayService — YARP routing + JWT authentication
- [x] UserService — Staff management, scheduling, clocking in/out, and hours reports
- [x] BillingService — GST invoice generation and settlements
- [x] InventoryService — Stock levels, adjustments, and deduction logic
- [x] ReportService — Materialized views, revenue dashboards, and ingredient analytics
- [ ] SignalR — push real-time updates to frontend KDS
- [ ] Unit + integration tests per service
- [ ] Docker Compose for non-Aspire deployment
- [ ] Azure Container Apps deployment guide

---

## Contributing

This project is structured as a guided learning project. Each service is built incrementally following the same clean architecture pattern. If you're following along, each service introduces one new concept:

| Service | New Concept |
|---|---|
| MenuService | Clean Architecture baseline, EF Core, MediatR |
| TableService | Domain events, MassTransit publish |
| OrderService | Aggregate with child entities, event publishing |
| KitchenService | MassTransit consumer, cross-service event handling |
| BillingService | Complex business logic, GST calculation |
| InventoryService | Event-driven side effects |
| ReportService | Read models, event sourcing lite |
| UserService | Staff authentication, shifts, and schedules |
| GatewayService | YARP, JWT, cross-cutting concerns |

---

## License

MIT License — free to use for learning, portfolio, or production.

---

<div align="center">
  Built with .NET 10 · Aspire 13 · RabbitMQ · SQL Server
</div>
