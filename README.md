# Connector Information Gathering Tool (CIGT)

Enterprise internal application for Implementation Engineers to collect application integration details from customers and manage Connector Requirement Gathering projects.

## Tech Stack

| Layer | Technology |
|-------|------------|
| Frontend | Angular 20, Angular Material, Reactive Forms, Signals, SCSS, RxJS |
| Backend | ASP.NET Core 10 Web API |
| Auth | ASP.NET Core Identity + JWT (+ refresh tokens) |
| Data | Entity Framework Core + SQL Server |

## Project Structure

```
├── frontend/                 # Angular 20 SPA
└── API/                      # ASP.NET Core 10 Web API
    ├── Configuration/        # Strongly typed settings (Jwt)
    ├── Controllers/
    ├── Data/                 # ApplicationDbContext, DbSeeder
    ├── DTOs/
    ├── Helpers/
    ├── Middleware/
    ├── Migrations/
    ├── Models/
    ├── Repositories/         # EF Core repositories
    └── Services/
```

## Prerequisites

- Node.js 20+ and npm
- .NET 10 SDK
- SQL Server 2022 (Docker recommended)
- `dotnet-ef` tool (`dotnet tool install --global dotnet-ef`)

## Database (local)

SQL Server via Docker example:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=StrongPassword@123" \
  -p 1433:1433 --name sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

Create empty database `cigt` in DBeaver (or let migrations create it — ensure the login can create the DB / DB already exists).

Connection string (Development only — `API/appsettings.Development.json`):

```
Server=localhost,1433;Database=cigt;User Id=sa;Password=StrongPassword@123;TrustServerCertificate=True;
```

## Getting Started

### Backend

```bash
cd API
dotnet ef database update   # applies migrations (also runs automatically on startup via DbSeeder)
dotnet run --launch-profile http
```

API: **http://localhost:5189**

### Frontend

```bash
cd frontend
npm install
npm start
```

App: **http://localhost:4200**

## Authentication

| Field | Value |
|-------|-------|
| Email | `admin@arconnet.com` |
| Username | `admin` |
| Password | `Password123` |

All company emails must end with **`@arconnet.com`**.

## Auth API

- `POST /api/auth/login`
- `POST /api/auth/register`
- `POST /api/auth/forgot-password` (placeholder)
- `POST /api/auth/reset-password` (placeholder)
- `POST /api/auth/verify-email` (placeholder)
- `POST /api/auth/refresh`
- `POST /api/auth/logout`
- `GET  /api/auth/me`
- `PUT  /api/auth/profile`
- `POST /api/auth/change-password`

## Core API Endpoints

- `GET/POST/DELETE /api/clients`
- `GET/POST/PUT/DELETE /api/projects`
- `POST /api/projects/{id}/generate-link`
- `GET/POST /api/customer-form/{token}` (anonymous)
- `GET/POST/DELETE /api/attachments/...`
- `GET/POST /api/projects/{id}/documents...`

## Document output

After completing the Connector Information Gathering form:

1. **Generate PDF** — backend builds a professional PDF (QuestPDF) and appends uploaded PDF/PNG/JPG files into one consolidated document.
2. **Share via Email** — downloads an Outlook `.eml` draft addressed to the Connector Team, with subject/body filled and the PDF already attached. Open the `.eml` and click Send.

Config (`Email:ConnectorTeamAddress`): default `connector-team@arconnet.com`.

Endpoints:
- `POST /api/projects/{id}/generate-pdf`
- `POST /api/projects/{id}/share-email`
- `POST /api/attachments/project/{id}/file` (multipart upload for PDF merge)

## Migrations

```bash
cd API
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Schema is managed only through EF Core migrations. Production: set connection string + JWT key via environment variables, deploy, run migrations (or rely on startup `MigrateAsync`).

## Production configuration

Set environment variables (do not commit secrets):

```
ConnectionStrings__DefaultConnection=Server=...;Database=cigt;User Id=...;Password=...;TrustServerCertificate=True;
Jwt__Key=<long-random-secret-at-least-32-chars>
Jwt__Issuer=CIGT
Jwt__Audience=CIGT.Users
ASPNETCORE_ENVIRONMENT=Production
```

## Design

- Montserrat typography (ARCON brand)
- White background, blue accents, light grey borders
- Responsive layout
