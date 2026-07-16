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
