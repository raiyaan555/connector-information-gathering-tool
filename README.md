# Connector Requirement Gathering Portal

Enterprise-grade internal application for Implementation Engineers to collect application integration details from customers and manage Connector Requirement Gathering projects.

## Tech Stack

| Layer | Technology |
|-------|------------|
| Frontend | Angular 20, Angular Material, Reactive Forms, Signals, SCSS, RxJS |
| Backend | ASP.NET Core 10 Web API |
| Data | In-memory collections (no database) |

## Project Structure

```
├── frontend/          # Angular 20 SPA
│   └── src/app/
│       ├── auth/      # Login, Register, Verify Email, Forgot Password
│       ├── dashboard/ # Project dashboard (Marketplace-style UI)
│       ├── projects/  # New project, project details
│       ├── customer-form/  # 8-step customer requirement form
│       ├── shared/    # Reusable components
│       ├── layouts/   # Auth & main layouts
│       ├── services/  # HTTP services
│       ├── guards/    # Auth & guest guards
│       └── interceptors/
└── API/               # ASP.NET Core 10 Web API
    ├── Controllers/
    ├── Services/
    ├── Repositories/
    ├── Models/
    └── DTOs/
```

## Getting Started

### Prerequisites

- Node.js 20+ and npm
- .NET 10 SDK

### Backend

```bash
cd API
dotnet run --launch-profile http
```

API runs at **http://localhost:5189**

### Frontend

```bash
cd frontend
npm install
npm start
```

App runs at **http://localhost:4200**

## Authentication (Dummy)

| Field | Value |
|-------|-------|
| Email | `admin@theconnector.com` |
| Password | `Password123` |

Email must end with `@theconnector.com` for all auth flows.

## Routes

| Route | Description |
|-------|-------------|
| `/login` | Sign in |
| `/register` | Create account |
| `/verify-email` | Email verification success |
| `/forgot-password` | Password reset |
| `/dashboard` | Project dashboard |
| `/project/new` | Create new project |
| `/project/:id` | Project details |
| `/form/:token` | Customer requirement form (public) |
| `/not-found` | 404 page |

## API Endpoints

- `POST /api/auth/login`
- `POST /api/auth/register`
- `GET /api/projects`
- `POST /api/projects`
- `POST /api/projects/{id}/generate-link`
- `GET /api/customer-form/{token}`
- `POST /api/customer-form/{token}`
- `GET /api/attachments/project/{projectId}`

## Phase 1 Scope

- Beautiful Marketplace-inspired UI
- Full navigation and routing
- Dummy authentication
- Mock APIs with in-memory data
- Customer form with 8-step Material stepper
- No database, document generation, email, or file storage

## Design

- Montserrat typography (matching ARCON brand)
- White background, blue accents, light grey borders
- Rounded cards with soft shadows
- Collapsible left sidebar
- Responsive layout
