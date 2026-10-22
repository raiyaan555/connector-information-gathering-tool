# CIGT — instructions for coding agents

Connector Information Gathering Tool. Internal ARCON app. Implementation engineers replace the connector Word document with a web app: clients, projects, a questionnaire, file uploads, a branded PDF, and a share that opens Outlook on the web. Customers submit a public form at `/form/{token}` with no login.

Read `SKILLS.md` before editing code. Leadership context is `docs/CIGT-Technical-Brief.md`. Demo clicks are `DEMO.md`. Human setup is `README.md`.

## Keep the docs current

When a change alters behavior, routes, roles, storage, auth, PDF, setup, or production risk, update the matching docs in the same change:

| File | Update it when |
|---|---|
| `CLAUDE.md` | Agent rules or "easy to get wrong" behavior change |
| `SKILLS.md` | Implementation rules change. Also mention any new skill under `.cursor/skills/` |
| `README.md` | Features, status, setup, or roadmap change |
| `DEMO.md` | What a person clicks, logs in as, or starts up changes |
| `docs/CIGT-Technical-Brief.md` | Product shape or a production risk changes |

Do not add a new markdown file for facts these already cover. `frontend/README.md` is the Angular CLI template. Change it only when frontend run or build commands change.

## Hard rules

- Database is SQL Server via EF Core (`UseSqlServer`). Do not introduce MySQL, Postgres, or SQLite.
- File bytes go through `IFileStorageService` onto disk (`API/Uploads`). SQL Server stores attachment metadata only. Never add a `byte[]`, base64, or `varbinary` column for PDFs or images.
- PDF only. Do not restore Word or PowerPoint export.
- New HTTP features follow Controller → Service → Repository and return `ApiResponse<T>`.
- Public registration always assigns **Engineer**. It ignores any role on the request. Only an Admin may create Engineer, Viewer, or Admin accounts, from `/users` and `POST api/users`.
- Do not commit secrets, `Uploads/`, `bin/`, `obj/`, or `node_modules/`.
- Do not seed extra sample clients. Do not log reset tokens, form tokens, or UAT passwords.

## Stack

| Piece | Use |
|---|---|
| Frontend | Angular 20, standalone components, Angular Material, SCSS, Signals, Reactive Forms |
| API | ASP.NET Core 10. Controllers, services, repositories. |
| Database | SQL Server. Dev: `docker compose up -d`, database `cigt`. |
| Auth | ASP.NET Core Identity, JWT access token, refresh tokens. Emails must end with `@arconnet.com`. |
| PDF | QuestPDF for the body. PdfSharpCore to merge uploaded PDFs and place PNG/JPG on pages. |
| Files | `IFileStorageService`. Provider today is `Local` only. |

Dev URLs: site `http://localhost:4200`, API `http://localhost:5189`, health `http://localhost:5189/health`.

## Layout

```text
frontend/src/app/     Angular app
API/Controllers/      HTTP only. Call a service. Return ApiResponse<T>.
API/Services/         Business rules.
API/Repositories/     EF Core queries.
API/Models/           Entities.
API/DTOs/             Request and response types.
API/Data/             ApplicationDbContext, DbSeeder. Migrations in API/Migrations/.
```

Frontend calls `environment.apiUrl`. Do not hardcode hosts in components.

## Easy to get wrong

- Logged-in pages sit under `MainLayoutComponent` with `authGuard`. Login and register sit under `AuthLayoutComponent` with `guestGuard`. `/users` also uses `adminGuard`.
- `GET/POST api/users` check the Admin role inside `UserAdminService` and return 403 when the caller is not an Admin. Most other controllers only use `[Authorize]`. Viewer still cannot be blocked from edit or delete. A new destructive action needs an explicit role check.
- Email verification and forgot-password are placeholders. Do not claim they send mail. The server does not send SMTP. Share opens Outlook on the web (`https://outlook.office.com/mail/deeplink/compose`) to `Email:ConnectorTeamAddress` (CI Connectors Team, `ciconnectorsteam@arconnet.com`) with a message the engineer can edit. The PDF, or the CIGT zip when a JSON file exists, downloads in that same step. Outlook's compose link cannot carry a file, so the engineer drops the download onto the message and presses Send. A Jira ticket saved on the project (`PUT api/projects/{id}/jira-ticket`) is included in that message and on the PDF.
- New questionnaire fields must be added to `FormFieldCatalog` or they will not appear in the PDF.
- Allowed uploads are PDF, DOCX, JPG, JPEG, PNG, and JSON. The API rejects anything else. PDF, DOCX, and images are merged into the generated PDF. If a JSON file is stored, the download is `CIGT_{Client}_{Application}.zip` containing that PDF and the JSON files. With no JSON file, the download stays `CIGT_{Client}_{Application}.pdf`. A missing attachment file is skipped. Do not fail the whole PDF.
- `UploadAsync` (JSON, no bytes) is legacy. New uploads use `UploadFileAsync`.
- Workspace drafts in `localStorage` are a cache. SQL Server (`DocumentVersion`) is the source of truth after save.
- `DbSeeder` creates roles, `admin@arconnet.com`, and sample clients only when the client table is empty. Do not add more sample customers.
- Questionnaire JSON can hold UAT credentials in plain text. Do not add more secret fields. Do not log them.
- Production secrets stay in environment variables. Do not commit connection strings, JWT keys, or passwords.

## How to add a feature

Follow the steps in `SKILLS.md`: model and migration, repository, service (`ApiResponse<T>`), controller, Angular model and service, DI in `API/Program.cs`.

```bash
dotnet ef migrations add <Name> --project API --startup-project API
```

Do not edit `ApplicationDbContextModelSnapshot.cs` by hand. Do not edit the initial migration. The API runs `Database.MigrateAsync` on startup.

## Before you finish

- `dotnet build API/API.csproj` when the API changed.
- Exercise a UI change in the browser: the screen you changed and any page that reads the same state.
- Commit only when the user asks. Prefixes: `feat:`, `fix:`, `refactor:`, `docs:`, `chore:`.

## Out of scope unless the user asks

Audit log, SMTP, Azure Blob implementation, role checks on every existing endpoint, Word export, swapping SQL Server. The Users page (list and create) exists. A broader admin dashboard does not.
