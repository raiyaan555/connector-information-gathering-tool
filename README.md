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

- 👥 Team Development Workflow

This project follows a simple Git workflow to keep development organized and avoid merge conflicts.

Branch Structure
main
│
├── Stable production-ready code only
│
develop
│
├── Integration branch
│
├── feature/frontend-ui
├── feature/backend-api
├── feature/database
├── feature/document-generation
└── feature/file-management
Branch Purpose
Branch	Purpose
main	Stable, production-ready code only. Never commit directly here.
develop	Integration branch where all completed features are merged.
feature/*	Individual feature development branches.
👨‍💻 Team Responsibilities
Developer	Responsibility
Frontend Developer	Angular UI, Components, Forms
Backend Developer	ASP.NET Core APIs
Database Developer	Repository Layer, Entity Framework, MySQL Integration

Each developer should work only on their own feature branch.

🚀 Getting Started for Developers
1. Clone the Repository
git clone <repository-url>
cd connector-information-gathering-tool
2. Checkout the Develop Branch
git checkout develop
git pull origin develop

Always start from the latest develop branch.

3. Create Your Feature Branch

Examples:

Frontend

git checkout -b feature/frontend-ui

Backend

git checkout -b feature/backend-api

Database

git checkout -b feature/database

Push your branch to GitHub

git push -u origin feature/your-branch-name
💻 Daily Development Workflow

Before starting work every day

git checkout develop
git pull origin develop

Switch back to your branch

git checkout feature/your-branch

Merge the latest changes from develop

git merge develop

Now continue coding.

📤 Committing Code

After completing a task

Stage your changes

git add .

Commit

git commit -m "feat: implement attachment upload"

Push

git push
✅ Commit Message Convention

Please follow these prefixes.

New Feature
feat: add project workspace
Bug Fix
fix: resolve login validation issue
UI Improvements
style: improve dashboard spacing
Code Refactoring
refactor: split information form into reusable components
Documentation
docs: update README
Dependency Updates
chore: update Angular packages
🔀 Creating a Pull Request

After pushing your branch

Open GitHub.
Open your branch.
Click Compare & Pull Request.
Create the Pull Request.

Merge into

develop

Do NOT merge directly into main.

⚠️ Important Rules

Please follow these rules while contributing.

✅ Always
Pull the latest changes before starting work.
Work only on your assigned feature branch.
Use meaningful commit messages.
Test your changes before pushing.
Create a Pull Request to develop.
❌ Never
Push directly to main.
Commit unfinished code to develop.
Delete other developers' branches.
Force push without discussing with the team.
Change another developer's feature branch.
🔄 Development Lifecycle
Clone Repository
        │
        ▼
Checkout develop
        │
        ▼
Create feature branch
        │
        ▼
Develop
        │
        ▼
Commit
        │
        ▼
Push
        │
        ▼
Create Pull Request
        │
        ▼
Review
        │
        ▼
Merge into develop
        │
        ▼
Testing
        │
        ▼
Merge develop → main
📂 Before Creating a Pull Request

Please verify the following:

Code compiles successfully.
No build errors.
No console errors.
UI works correctly.
No unnecessary files have been committed.
Commit messages follow the project convention.
The latest changes from develop have been merged into your branch.
