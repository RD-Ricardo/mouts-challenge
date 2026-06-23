# Developer Evaluation Project

`READ CAREFULLY`

## Use Case
**You are a developer on the DeveloperStore team. Now we need to implement the API prototypes.**

As we work with `DDD`, to reference entities from other domains, we use the `External Identities` pattern with denormalization of entity descriptions.

Therefore, you will write an API (complete CRUD) that handles sales records. The API needs to be able to inform:

* Sale number
* Date when the sale was made
* Customer
* Total sale amount
* Branch where the sale was made
* Products
* Quantities
* Unit prices
* Discounts
* Total amount for each item
* Cancelled/Not Cancelled

It's not mandatory, but it would be a differential to build code for publishing events of:
* SaleCreated
* SaleModified
* SaleCancelled
* ItemCancelled

If you write the code, **it's not required** to actually publish to any Message Broker. You can log a message in the application log or however you find most convenient.

### Business Rules

* Purchases above 4 identical items have a 10% discount
* Purchases between 10 and 20 identical items have a 20% discount
* It's not possible to sell above 20 identical items
* Purchases below 4 items cannot have a discount

These business rules define quantity-based discounting tiers and limitations:

1. Discount Tiers:
   - 4+ items: 10% discount
   - 10-20 items: 20% discount

2. Restrictions:
   - Maximum limit: 20 items per product
   - No discounts allowed for quantities below 4 items

## Overview
This section provides a high-level overview of the project and the various skills and competencies it aims to assess for developer candidates. 

See [Overview](/.doc/overview.md)

## Tech Stack
This section lists the key technologies used in the project, including the backend, testing, frontend, and database components. 

See [Tech Stack](/.doc/tech-stack.md)

## Frameworks
This section outlines the frameworks and libraries that are leveraged in the project to enhance development productivity and maintainability. 

See [Frameworks](/.doc/frameworks.md)

<!-- 
## API Structure
This section includes links to the detailed documentation for the different API resources:
- [API General](./docs/general-api.md)
- [Products API](/.doc/products-api.md)
- [Carts API](/.doc/carts-api.md)
- [Users API](/.doc/users-api.md)
- [Auth API](/.doc/auth-api.md)
-->

## Project Structure
This section describes the overall structure and organization of the project files and directories. 

See [Project Structure](/.doc/project-structure.md)

---

## Implementation

The backend lives under [backend/](backend/) and follows Clean Architecture + DDD:

```
backend/
├── src/
│   ├── Ambev.DeveloperEvaluation.Domain        # Entities, validators, repository interfaces, domain events
│   ├── Ambev.DeveloperEvaluation.Application   # CQRS handlers (MediatR), AutoMapper profiles
│   ├── Ambev.DeveloperEvaluation.ORM           # EF Core (Npgsql) DbContext, configurations, migrations
│   ├── Ambev.DeveloperEvaluation.WebApi        # ASP.NET Core controllers, request/response DTOs
│   ├── Ambev.DeveloperEvaluation.Common        # JWT, BCrypt, FluentValidation glue
│   └── Ambev.DeveloperEvaluation.IoC           # Module initializers (DI registration)
└── tests/
    └── Ambev.DeveloperEvaluation.Unit          # xUnit + NSubstitute + Bogus
```

### Sales feature (use case under evaluation)

- `Sale` is an aggregate root, `SaleItem` is a child entity. External identities are denormalized: `CustomerName`, `BranchName`, `ProductName` are stored on the sale.
- Discount rules encapsulated on the entity ([SaleItem.DiscountRateFor](backend/src/Ambev.DeveloperEvaluation.Domain/Entities/SaleItem.cs)):
  - `qty < 4`  → 0%
  - `4 ≤ qty ≤ 9`  → 10%
  - `10 ≤ qty ≤ 20` → 20%
  - `qty > 20` → throws `DomainException`
- Domain events ([SaleEvents](backend/src/Ambev.DeveloperEvaluation.Domain/Events/SaleEvents.cs)) are wrapped as MediatR notifications in [Application/Sales/Events](backend/src/Ambev.DeveloperEvaluation.Application/Sales/Events) and logged via Serilog by [SaleEventLogger](backend/src/Ambev.DeveloperEvaluation.Application/Sales/Events/SaleEventLogger.cs):
  - `SaleCreated`, `SaleModified`, `SaleCancelled`, `ItemCancelled`.
- Errors are returned in the documented `{ type, error, detail }` shape via [ValidationExceptionMiddleware](backend/src/Ambev.DeveloperEvaluation.WebApi/Middleware/ValidationExceptionMiddleware.cs).
- Pagination, ordering and filtering follow the spec from [.doc/general-api.md](.doc/general-api.md) (`_page`, `_size`, `_order`, `_min*`/`_max*`, `*` wildcards) via [QueryableExtensions](backend/src/Ambev.DeveloperEvaluation.ORM/Extensions/QueryableExtensions.cs).

### Endpoints

All under `/api/sales` (require JWT — log in at `POST /api/auth/login` first):

| Method | Route | Purpose |
| --- | --- | --- |
| POST   | `/api/sales`                              | Create a sale |
| GET    | `/api/sales/{id}`                         | Get a sale by id (with items) |
| GET    | `/api/sales`                              | List sales (paginated, filtered, ordered) |
| PUT    | `/api/sales/{id}`                         | Update sale header + replace items |
| PATCH  | `/api/sales/{id}/cancel`                  | Cancel a sale (soft) |
| PATCH  | `/api/sales/{id}/items/{itemId}/cancel`   | Cancel a single item (recalculates total) |
| DELETE | `/api/sales/{id}`                         | Delete a sale (hard) |

### Running locally

Prerequisites: .NET 8 SDK, Docker.

```powershell
cd backend

# 1. Start Postgres (and the optional Mongo/Redis services)
docker compose up -d ambev.developerevaluation.database

# 2. Restore EF tools (pinned to 8.0.10 via .config/dotnet-tools.json)
dotnet tool restore

# 3. Apply migrations
dotnet dotnet-ef database update -p src/Ambev.DeveloperEvaluation.ORM -s src/Ambev.DeveloperEvaluation.WebApi

# 4. Run the API
dotnet run --project src/Ambev.DeveloperEvaluation.WebApi
```

Swagger UI is exposed at the development URL printed by Kestrel (typically `https://localhost:8081/swagger`).

### Tests

```powershell
cd backend
dotnet test
```

73 unit tests cover the discount rules, item/sale cancellation, validator behavior, and the create/cancel/cancel-item handlers.

### Out of scope (next steps)

- Real publishing to Rebus / RabbitMQ (currently logged only — per the spec this is optional).
- Products / Carts / extended Users endpoints from [.doc/](.doc/) — the Sales aggregate is the required use case; the other resources are documented for completeness.
- Angular frontend.

