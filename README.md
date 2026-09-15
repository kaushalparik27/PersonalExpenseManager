# Personal Expense Manager

A premium personal finance web application: accounts, income/expense tracking, transfers, categories, recurring transactions, dashboards, and reports.

**Status:** Phases 1–7 (Foundation, Authentication, Database, Accounts, Categories, Transactions, Dashboard) complete. Detailed reporting/drilldown and the recurring-transaction engine are not yet implemented — see [Development Phases](#development-phases).

## Stack

- **Backend:** ASP.NET Core (.NET 10) Web API, EF Core, ASP.NET Core Identity, PostgreSQL (Npgsql), FluentValidation, Swagger.
- **Frontend:** React 19, TypeScript, Vite, Tailwind CSS, shadcn/ui, TanStack Query, React Router, React Hook Form, Zod, Recharts.

## 1. Prerequisites

- [.NET SDK 10.0+](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) and npm
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for local PostgreSQL)

## 2. PostgreSQL Setup

PostgreSQL runs via Docker Compose. Copy the example env file and adjust credentials if you like:

```bash
cp .env.example .env
```

Start the database:

```bash
docker compose up -d postgres
```

This creates a `personal_expense_manager` database with a persisted volume (`pem-postgres-data`).

## 3. Environment Configuration

- Backend connection string lives in `src/PersonalExpenseManager.Api/appsettings.Development.json` (`ConnectionStrings:DefaultConnection`). The default local-dev credentials match `.env.example` — update both together if you change them.
- Frontend API base URL lives in `client/.env.local` (`VITE_API_URL`). Copy from `client/.env.example` if it doesn't exist.

Never commit real production credentials — `appsettings.Development.json` only holds local dev-only defaults.

## 4. Database Migrations

EF Core migrations are managed with the `dotnet-ef` local tool (already restored via `dotnet tool restore`).

```bash
dotnet tool restore
dotnet ef database update --project src/PersonalExpenseManager.Infrastructure --startup-project src/PersonalExpenseManager.Api
```

To add a new migration after changing entities:

```bash
dotnet ef migrations add <MigrationName> --project src/PersonalExpenseManager.Infrastructure --startup-project src/PersonalExpenseManager.Api --output-dir Persistence/Migrations
```

## 5. Backend Startup

```bash
dotnet run --project src/PersonalExpenseManager.Api
```

Swagger UI is available at `/swagger` in development.

## 6. Frontend Startup

```bash
cd client
npm install
npm run dev
```

The app runs at `http://localhost:5173`.

## 7. Tests

```bash
dotnet test
```

Integration tests run against a separate `personal_expense_manager_test` database on the same Postgres container (schema is dropped and recreated automatically per test class) — no manual setup needed beyond having Postgres running.

## Demo Data

Running the API in Development automatically seeds a demo account (idempotent — safe to restart):

- Email: `demo@example.com`
- Password: `Demo!Passw0rd`

It comes with 4 accounts, expense/income categories, and ~4 months of sample transactions, so the dashboard/reports UI has realistic data to render once those features exist. This is local dev-only data, not a permanent system account.

Every new registration also gets a starter set of expense categories (Food, Transportation, Utilities, etc.) automatically.

## 8. Docker

`docker-compose.yml` provisions PostgreSQL only (the app itself runs natively for development). Stop it with:

```bash
docker compose down
```

Add `-v` to also remove the persisted data volume.

## Solution Structure

```
PersonalExpenseManager.sln
src/
├── PersonalExpenseManager.Domain          # Entities, enums, no framework dependencies
├── PersonalExpenseManager.Application     # DTOs, validators, application services
├── PersonalExpenseManager.Infrastructure  # EF Core, ASP.NET Core Identity, PostgreSQL
└── PersonalExpenseManager.Api             # Controllers, middleware, composition root
tests/
├── PersonalExpenseManager.UnitTests
└── PersonalExpenseManager.IntegrationTests
client/                                    # React + Vite frontend (feature-oriented src/features/*)
```

## Development Phases

Authentication is mandatory and precedes financial functionality:

1. **Foundation** *(done)* — solution, React/Vite, Tailwind, shadcn/ui, PostgreSQL Docker, EF Core, ASP.NET Core Identity, basic API infra.
2. **Authentication** *(done)* — registration, login, logout, forgot/reset password, protected routes, API authorization, user ownership tests.
3. **Database** *(done)* — categories, transactions, recurring transactions, tags, migrations, seed data.
4. **Accounts** *(done)* — CRUD, centralized balance calculation.
5. **Categories** *(done)* — expense/income hierarchy, archive, reorder.
6. **Transactions** *(done)* — expense/income/transfer entry, edit, soft delete, search, filtering, pagination.
7. **Dashboard** *(done)* — summary cards, Income vs Expense and category charts, recent transactions, insights.
8. **Reports** — income/expense/cash-flow/category/account analysis, drilldown.
9. **Recurring Transactions.**
10. **Security & UX Review.**
11. **Testing & Production Readiness.**

Each user-owned entity is scoped to the authenticated user; the API is the sole authorization boundary (frontend route guards are UX-only).
