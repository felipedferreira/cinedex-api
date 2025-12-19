# 🎬 Cinedex

**Cinedex** is a modern ASP.NET Core REST API that lets users explore, search, and manage movie data — inspired by platforms like IMDb and TMDb.  
It’s built with **Clean Architecture** and **Domain-Driven Design (DDD)** principles, focusing on scalability, maintainability, and clean separation of concerns.

---

## 🧩 Architecture Overview

Cinedex follows a **Classic Clean Architecture** approach:

```
Cinedex.sln
├── src/
│   ├── Cinedex.Domain/                 # Entities, Value Objects, Domain Events
│   ├── Cinedex.Application/            # Use Cases, Commands, Queries, Validation
│   ├── Cinedex.Application.Abstractions/ # Public Contracts and Shared Interfaces
│   ├── Cinedex.Infrastructure/         # EF Core, Repositories, Caching, Auth, etc.
│   └── Cinedex.Presentation/                 # REST Controllers / Endpoints
└── tests/
    ├── Cinedex.UnitTests/
    └── Cinedex.IntegrationTests/
```

### Dependency Flow

```
Presentation → Application
Infrastructure → Application.Abstractions
Application → Application.Abstractions
Application.Abstractions → Domain
Infrastructure → Application.Abstractions
```

---

## ⚙️ Tech Stack

| Layer | Technologies |
|-------|---------------|
| **Web API** | ASP.NET Core 9, Minimal API / Controllers |
| **Application** | MediatR (CQRS), FluentValidation, Mapster |
| **Infrastructure** | EF Core, SQL Server/PostgreSQL, Redis Cache |
| **Testing** | xUnit, Testcontainers |
| **Observability** | Serilog, ProblemDetails, OpenTelemetry (optional) |

---

## 🚀 Features

- 🎞️ **Movie Catalog** — Browse, search, and filter movies by title, genre, or release year.  
- 🧑‍🎤 **Cast & Crew** — Retrieve structured information about actors, directors, and roles.  
- ⭐ **Ratings System** — Track and aggregate user ratings and reviews.  
- ⚡ **Caching Support** — Hybrid caching (Memory + Redis) for faster response times.  
- 🧱 **Clean Architecture** — Strict separation between layers for maintainability.  
- 🧩 **DDD Concepts** — Aggregates, Value Objects, Domain Events, Repositories.  
- ✅ **Validation & Error Handling** — FluentValidation and standardized ProblemDetails.  
- 🔒 **Authentication Ready** — JWT / OAuth2 abstractions for secure endpoints.  

---


## 🧱 Code Style & Build Configuration

Common build settings (nullable, analyzers, etc.) are shared using:

- `Directory.Build.props` — shared compiler and analyzer configuration  
- `Directory.Build.targets` — shared build logic and hooks  

Each project automatically inherits these settings for consistency.

---


## 📄 License

This project is licensed under the [MIT License](LICENSE).

---