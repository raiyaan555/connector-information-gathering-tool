# 🚀 Connector Information Gathering Tool (CIGT)

> Enterprise-grade Angular 20 + ASP.NET Core 10 application for replacing the traditional Connector Information Gathering Word document with a modern web-based application.

---

# 📖 Overview

The **Connector Information Gathering Tool (CIGT)** is an internal enterprise application designed for **Implementation Engineers** to collect, manage, review, and generate Connector Information Gathering Documents through an intuitive web interface.

Instead of manually filling Microsoft Word documents, engineers complete a structured, interactive UI that supports:

- Project & Client Management
- Connector Information Collection
- Draft Saving
- Professional PDF Generation
- File Upload Management (PDF, DOCX, JPG, PNG, JSON)
- Review Screen & Version History
- Share to the CI Connectors Team (Outlook on the web)
- SQL Server Persistence (EF Core)
- JWT Authentication (ASP.NET Identity)

---

# 🛠 Technology Stack

| Layer | Technology |
|---------|------------|
| Frontend | Angular 20 |
| UI Framework | Angular Material |
| State Management | Angular Signals |
| Forms | Reactive Forms |
| Styling | SCSS |
| Backend | ASP.NET Core 10 Web API |
| Auth | ASP.NET Core Identity + JWT + Refresh Tokens |
| ORM | Entity Framework Core |
| Database | SQL Server |
| PDF Generation | QuestPDF + PdfSharpCore |
| File Storage | Local disk (`Uploads/`, metadata in SQL Server) |
| Language | TypeScript, C# |

---

# 📂 Project Structure

```text
connector-information-gathering-tool/
├── frontend/
│   └── src/app/
│       ├── auth/
│       ├── dashboard/
│       ├── clients/
│       ├── projects/
│       ├── users/
│       ├── shared/
│       ├── layouts/
│       ├── services/
│       ├── guards/
│       ├── interceptors/
│       └── models/
│
├── API/
│   ├── Controllers/
│   ├── Services/
│   ├── Repositories/
│   ├── Data/
│   ├── Migrations/
│   ├── Configuration/
│   ├── DTOs/
│   ├── Models/
│   ├── Assets/
│   ├── Uploads/            # local files (gitignored)
│   └── Program.cs
│
├── docker-compose.yml      # SQL Server with a persistent volume
├── DEMO.md                 # step-by-step demo script
├── README.md
└── .gitignore
```

---

# ✨ Features

## Current Features

- ✅ Login & Authentication (JWT + Refresh Tokens)
- ✅ Register / Forgot Password / Profile APIs
- ✅ Role seeding (Admin, Engineer, Viewer)
- ✅ Admin Users page (list accounts, create Engineer / Viewer / Admin)
- ✅ Public registration always creates an Engineer
- ✅ Dashboard
- ✅ Client Management
- ✅ Create New Project / Application
- ✅ Connector Information Gathering Workspace
- ✅ Multi-Step Information Gathering Form
- ✅ Save Draft
- ✅ Review Screen (expandable sections)
- ✅ Version History (Version 1 + Change Requests)
- ✅ Professional PDF Generation
- ✅ Uploaded PDFs, DOCX files, and images merged into the generated PDF
- ✅ JSON files packaged with that PDF in a `CIGT_Client_Application.zip` download
- ✅ Share to Connector Team (Outlook on the web, addressed to the CI Connectors Team group, with an editable message). The PDF, or the CIGT zip when JSON files exist, downloads so the engineer can drop it onto that message
- ✅ Jira ticket saved on the application, included in the share message and on the PDF
- ✅ File Upload (stored on disk + metadata in DB)
- ✅ SQL Server + Entity Framework Core
- ✅ Responsive UI
- ✅ Professional Angular Material Design

## 🚧 Upcoming Features

- Cloud Blob Storage (Azure Blob / AWS S3)
- Audit Logs
- SMTP Email Sending
- Expanded Role-Based Access Control (Users page is Admin-only; other screens are still open to any signed-in user)
- Broader admin dashboard (beyond the Users page)

> **Note:** The tool does not generate Word or PowerPoint files. PDF is the document it creates. An uploaded `.docx` is merged into that PDF. When a JSON file is attached, the download is a zip named `CIGT_Client_Application` that contains the PDF and the JSON files.

---

# 🚀 Getting Started

## Prerequisites

- Node.js 20+
- npm
- .NET 10 SDK
- Docker Desktop (recommended for SQL Server)
- Visual Studio Code / Visual Studio 2022

---

## Database (SQL Server)

Start SQL Server with a persistent Docker volume so users and projects survive container restarts:

```bash
docker compose up -d
```

This creates container `cigt-sqlserver` on port `1433` with volume `cigt-sql-data`.

The development connection string is configured in:

```
API/appsettings.Development.json
```

```
Server=localhost,1433;Database=cigt;User Id=sa;Password=StrongPassword@123;TrustServerCertificate=True;
```

Entity Framework Core migrations run automatically on API startup. A database health check is available at `http://localhost:5189/health`.

---

## Backend

```bash
cd API

dotnet restore

dotnet run --launch-profile http
```

Backend URL:

```
http://localhost:5189
```

---

## Frontend

```bash
cd frontend

npm install

npm start
```

Frontend URL:

```
http://localhost:4200
```

---


> All users must register using an **@arconnet.com** email address. Self-registration always creates an **Engineer**. An Admin creates Viewer or Admin accounts from the **Users** page (`/users`).

Password policy (enforced in the UI and the API):

- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one digit

A step-by-step demo script is in `DEMO.md`.

## Production configuration

Production values live in environment variables and `API/appsettings.Production.json`. No code change is required to point at a real SQL Server, a production JWT key, a CORS origin, or an absolute uploads folder.

```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=YOUR_SERVER;Database=cigt;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;
Jwt__Key=your-production-secret-at-least-32-characters-long
Storage__Provider=Local
Storage__RootPath=C:\inetpub\cigt\Uploads
Cors__AllowedOrigins__0=https://your-production-domain.com
App__FrontendBaseUrl=https://your-production-domain.com
```

`Storage:Provider` is `Local` today. A cloud backend can be added later by implementing `IFileStorageService` and changing that setting.

---

# 🧭 Application Workflow

```text
Login
      │
      ▼
Dashboard
      │
      ▼
Create Client / Project
      │
      ▼
Connector Information Gathering Form
      │
      ▼
Save Draft
      │
      ▼
Generate PDF
      │
      ▼
Review Screen
      │
      ├── Download PDF
      ├── Share to Connector Team
      └── Edit Project
             │
             ▼
      Creates New Version
      (Version History + Change Request)
```

---

# 📌 Project Status

| Module | Status |
|---------|--------|
| Frontend UI | ✅ Completed |
| Authentication UI | ✅ Completed |
| JWT Authentication API | ✅ Completed |
| Admin Users page | ✅ Completed |
| Role checks on every screen | 🚧 Pending (Users is Admin-only; other APIs only require login) |
| Dashboard | ✅ Completed |
| Information Gathering Form | ✅ Completed |
| Save Draft | ✅ Completed |
| Review Screen | ✅ Completed |
| Version History / Change Requests | ✅ Completed |
| ASP.NET Core APIs | ✅ Completed |
| SQL Server + EF Core | ✅ Completed |
| File Storage (`Uploads/`) | ✅ Completed |
| PDF Generation | ✅ Completed |
| Share to Connector Team (Outlook on the web) | ✅ Completed |
| Jira ticket on the application | ✅ Completed |
| Word Generation | ❌ Removed |
| Audit Logs | 🚧 Pending |
| Cloud Storage | 🚧 Pending |

---

# 👥 Team Development Workflow

This project follows a **Git Feature Branch Workflow**.

## Branch Structure

```text
main
│
├── Stable Releases
│
develop
│
├── Integration Branch
│
├── feature/frontend-ui
├── feature/backend-api
├── feature/database
├── feature/document-generation
└── feature/file-management
```

### Branch Responsibilities

| Branch | Purpose |
|---------|----------|
| `main` | Stable production-ready code |
| `develop` | Integration branch |
| `feature/*` | Individual feature development |

> ⚠ Never commit directly to `main`.

---

# 👨‍💻 Team Responsibilities

| Developer | Responsibility |
|------------|----------------|
| Frontend Developer | Angular UI, Components, Forms |
| Backend Developer | ASP.NET Core APIs |
| Database Developer | EF Core, SQL Server, Repository Layer |

Each developer should work only within their own feature branch.

---

# 🚀 Developer Setup

## 1. Clone Repository

```bash
git clone https://github.com/raiyaan555/connector-information-gathering-tool.git

cd connector-information-gathering-tool
```

---

## 2. Switch to Develop

```bash
git checkout develop

git pull origin develop
```

---

## 3. Create Feature Branch

Frontend

```bash
git checkout -b feature/frontend-ui
```

Backend

```bash
git checkout -b feature/backend-api
```

Database

```bash
git checkout -b feature/database
```

Push your branch:

```bash
git push -u origin feature/your-branch-name
```

---

# 💻 Daily Development Workflow

```bash
git checkout develop

git pull origin develop

git checkout feature/your-branch

git merge develop
```

Then begin development.

---

# 📤 Commiting Code

```bash
git add .

git commit -m "feat: implement attachment upload"

git push
```

---

# 📝 Commit Convention

| Prefix | Purpose |
|----------|----------|
| feat | New Feature |
| fix | Bug Fix |
| refactor | Refactoring |
| style | UI / Styling |
| docs | Documentation |
| chore | Dependencies / Maintenance |

Examples:

```text
feat: implement project workspace

fix: resolve login validation

style: improve dashboard layout

refactor: split workspace into standalone components

docs: update README

chore: update Angular packages
```

---

# 🔀 Pull Request Workflow

1. Push your feature branch.
2. Open GitHub.
3. Click **Compare & Pull Request**.
4. Create a Pull Request.
5. Merge into **develop**.

> ❌ Never merge directly into `main`.

---

# ⚠ Contribution Rules

## ✅ Always

- Pull latest changes before starting work.
- Work only on your assigned feature branch.
- Use meaningful commit messages.
- Test before pushing.
- Create Pull Requests into `develop`.

## ❌ Never

- Push directly to `main`.
- Commit unfinished work into `develop`.
- Delete another developer's branch.
- Force push without discussion.
- Modify another developer's branch.

---

# 🔄 Development Lifecycle

```text
Clone Repository
        │
        ▼
Checkout develop
        │
        ▼
Create Feature Branch
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
```

---

# ✅ Pull Request Checklist

Before creating a Pull Request:

- [ ] Project builds successfully
- [ ] No build errors
- [ ] No console errors
- [ ] UI tested
- [ ] No unnecessary files committed (`bin/`, `obj/`, `App_Data/`)
- [ ] Commit messages follow project convention
- [ ] Latest `develop` merged into your branch

---

# 📅 Future Roadmap

- ✅ JWT Authentication
- ✅ SQL Server Support
- ✅ Entity Framework Core
- ✅ Professional PDF Generation
- ✅ Version History
- ✅ Change Requests
- ✅ Local File Storage
- 🚧 Audit Logs
- 🚧 SMTP Email Notifications
- 🚧 Cloud Blob Storage
- 🚧 Expanded Role-Based Access Control
- 🚧 Broader admin dashboard (Users page is done)

---

# 📄 License

**Internal Project – ARCON Tech Solutions**
