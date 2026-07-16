# 🚀 Connector Information Gathering Tool (CIGT)

> Enterprise-grade Angular 20 + ASP.NET Core 10 application for replacing the traditional Connector Information Gathering Word document with a modern web-based application.

---

# 📖 Overview

The **Connector Information Gathering Tool (CIGT)** is an internal enterprise application designed for **Implementation Engineers** to collect, manage, review, and generate Connector Information Gathering Documents through an intuitive web interface.

Instead of manually filling Microsoft Word documents, engineers complete a structured, interactive UI that supports:

- Project Management
- Connector Information Collection
- Draft Saving
- Document Generation (Word / PDF)
- File Upload Management
- Future Database Integration

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
| Storage (Current) | Local Storage / In-Memory |
| Storage (Future) | MySQL / SQL Server |
| Language | TypeScript, C# |

---

# 📂 Project Structure

```text
connector-information-gathering-tool/

├── frontend/
│   └── src/app/
│       ├── auth/
│       ├── dashboard/
│       ├── projects/
│       ├── workspace/
│       ├── shared/
│       ├── layouts/
│       ├── services/
│       ├── guards/
│       ├── interceptors/
│       ├── models/
│       └── components/
│
├── API/
│   ├── Controllers/
│   ├── Services/
│   ├── Repositories/
│   ├── DTOs/
│   ├── Models/
│   └── Program.cs
│
├── docs/
│
├── README.md
└── .gitignore
```

---

# ✨ Features

## Current Features

- ✅ Login & Authentication (Dummy)
- ✅ Dashboard
- ✅ Create New Project
- ✅ Connector Information Gathering Workspace
- ✅ Multi-Step Information Gathering Form
- ✅ Save Draft
- ✅ Final Save
- ✅ Local Storage
- ✅ File Upload
- ✅ Responsive UI
- ✅ Professional Angular Material Design

---

# 🚧 Upcoming Features

- Word Generation (.docx)
- PDF Generation
- MySQL Integration
- SQL Server Integration
- Entity Framework Core
- Authentication API
- File Storage
- Version History
- Change Requests
- Audit Logs

---

# 🚀 Getting Started

## Prerequisites

- Node.js 20+
- npm
- .NET 10 SDK
- Visual Studio Code / Visual Studio 2022

---

## Backend

```bash
cd API

dotnet restore

dotnet run --launch-profile http
```

Runs on

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

Runs on

```
http://localhost:4200
```

---

# 🔐 Dummy Login

| Field | Value |
|---------|--------|
| Email | admin@theconnector.com |
| Password | Password123 |

> Email must end with **@theconnector.com**

---

# 🧭 Current Application Workflow

```text
Login
      │
      ▼
Dashboard
      │
      ▼
Create Project
      │
      ▼
Connector Information Gathering Workspace
      │
      ▼
Save Draft
      │
      ▼
Review
      │
      ▼
Save
      │
      ▼
Project Documents & Files
      │
      ▼
Generate Word / PDF
```

---

# 📌 Project Status

| Module | Status |
|----------|--------|
| Frontend UI | ✅ Completed |
| Authentication UI | ✅ Completed |
| Dashboard | ✅ Completed |
| Information Gathering Form | ✅ Completed |
| Save Draft | ✅ Completed |
| Local Storage | ✅ Completed |
| ASP.NET Core APIs | 🚧 Pending |
| Database Integration | 🚧 Pending |
| Word Generation | 🚧 Pending |
| PDF Generation | 🚧 Pending |

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

---

## Branch Responsibilities

| Branch | Purpose |
|----------|---------|
| **main** | Stable production-ready code only |
| **develop** | Integration branch for completed features |
| **feature/*** | Individual feature development |

⚠ **Never commit directly to `main`.**

---

# 👨‍💻 Team Responsibilities

| Developer | Responsibility |
|------------|---------------|
| Frontend Developer | Angular UI, Components, Forms |
| Backend Developer | ASP.NET Core APIs |
| Database Developer | Repository Layer, EF Core, MySQL |

Every developer should work only on **their own feature branch**.

---

# 🚀 Developer Setup

## 1️⃣ Clone Repository

```bash
git clone <repository-url>

cd connector-information-gathering-tool
```

---

## 2️⃣ Switch to Develop

```bash
git checkout develop

git pull origin develop
```

---

## 3️⃣ Create Your Feature Branch

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

Push branch

```bash
git push -u origin feature/your-branch-name
```

---

# 💻 Daily Development Workflow

Every morning

```bash
git checkout develop

git pull origin develop

git checkout feature/your-branch

git merge develop
```

Now start coding.

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

Examples

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

After pushing your branch

1. Open GitHub
2. Open your branch
3. Click **Compare & Pull Request**
4. Create Pull Request

Merge into

```text
develop
```

❌ Never merge directly into **main**.

---

# ⚠ Contribution Rules

## ✅ Always

- Pull latest changes before starting work
- Work only on your assigned feature branch
- Use meaningful commit messages
- Test before pushing
- Create Pull Requests into **develop**

---

## ❌ Never

- Push directly to **main**
- Commit unfinished work to **develop**
- Delete another developer's branch
- Force push without discussion
- Modify another developer's branch

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

Before creating a Pull Request, verify:

- [ ] Project builds successfully
- [ ] No build errors
- [ ] No console errors
- [ ] UI tested
- [ ] No unnecessary files committed
- [ ] Commit messages follow project convention
- [ ] Latest `develop` merged into your branch

---

# 📅 Future Roadmap

- JWT Authentication
- MySQL Integration
- SQL Server Support
- Entity Framework Core
- Word Document Generation
- PDF Generation
- Version History
- Change Requests
- Audit Logs
- Email Notifications
- Role-Based Access Control
- Admin Dashboard

---

# 📄 License

Internal Project — ARCON Tech Solutions.
