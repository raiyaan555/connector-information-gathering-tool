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
- File Upload Management (PDF / PNG / JPG)
- Review Screen & Version History
- Share via Email (Outlook `.eml` draft)
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
| File Storage | Local disk (`App_Data`) |
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
│   └── Program.cs
│
├── README.md
└── .gitignore
```

---

# ✨ Features

## Current Features

- ✅ Login & Authentication (JWT + Refresh Tokens)
- ✅ Register / Forgot Password / Profile APIs
- ✅ Role seeding (Admin, Engineer, Viewer)
- ✅ Dashboard
- ✅ Client Management
- ✅ Create New Project / Application
- ✅ Connector Information Gathering Workspace
- ✅ Multi-Step Information Gathering Form
- ✅ Save Draft
- ✅ Review Screen (expandable sections)
- ✅ Version History (Version 1 + Change Requests)
- ✅ Professional PDF Generation
- ✅ Uploaded PDFs merged into generated PDF
- ✅ Uploaded images placed on separate pages
- ✅ Share via Email (Outlook `.eml` with PDF attached)
- ✅ File Upload (stored on disk + metadata in DB)
- ✅ SQL Server + Entity Framework Core
- ✅ Responsive UI
- ✅ Professional Angular Material Design

## 🚧 Upcoming Features

- Cloud Blob Storage (Azure Blob / AWS S3)
- Audit Logs
- SMTP Email Sending
- Expanded Role-Based Access Control
- Admin Dashboard

> **Note:** Word (`.docx`) and PowerPoint generation have been intentionally removed. PDF is the only supported export format.

---

# 🚀 Getting Started

## Prerequisites

- Node.js 20+
- npm
- .NET 10 SDK
- SQL Server 2022 (Local or Docker)
- Visual Studio Code / Visual Studio 2022

---

## Database (SQL Server)

Example Docker setup:

```bash
docker run \
-e "ACCEPT_EULA=Y" \
-e "MSSQL_SA_PASSWORD=StrongPassword@123" \
-p 1433:1433 \
--name cigt-sql \
-d mcr.microsoft.com/mssql/server:2022-latest
```

The development connection string is configured in:

```
API/appsettings.Development.json
```

Entity Framework Core migrations run automatically on API startup.

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


> All users must register using an **@arconnet.com** email address.

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
| Dashboard | ✅ Completed |
| Information Gathering Form | ✅ Completed |
| Save Draft | ✅ Completed |
| Review Screen | ✅ Completed |
| Version History / Change Requests | ✅ Completed |
| ASP.NET Core APIs | ✅ Completed |
| SQL Server + EF Core | ✅ Completed |
| File Storage (`App_Data`) | ✅ Completed |
| PDF Generation | ✅ Completed |
| Share via Email (.eml) | ✅ Completed |
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
- 🚧 Admin Dashboard

---

# 📄 License

**Internal Project – ARCON Tech Solutions**
