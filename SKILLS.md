# CIGT development skill

Follow this file when changing the Connector Information Gathering Tool (CIGT), including production work. Read the matching code before editing. Copy existing patterns. Do not invent a second architecture.

Product context for leadership is in `docs/CIGT-Technical-Brief.md`. Agent instructions are in `CLAUDE.md`. This file is for implementation.

## Product

Internal ARCON tool. Implementation engineers replace the Connector Information Gathering Word document with a web app: clients, projects, a connector questionnaire, file uploads, a branded PDF, and a share that opens Outlook on the web for the connector team. Customers submit a public form at `/form/{token}` with no login.

PDF is the only export. Do not add Word or PowerPoint generation.

## Stack (do not swap)

| Piece | Use |
|---|---|
| Frontend | Angular 20, standalone components, Angular Material, SCSS, Signals, Reactive Forms |
| API | ASP.NET Core 10, C#, controllers + services + repositories |
| Database | **SQL Server** (EF Core `UseSqlServer`). Not MySQL, not Postgres, not SQLite. |
| Auth | ASP.NET Core Identity, JWT access token, refresh tokens. Emails must end with `@arconnet.com`. |
| PDF | QuestPDF for the document body. PdfSharpCore to merge uploaded PDFs and place PNG/JPG on pages. |
| Files | `IFileStorageService`. Today only `Local` (`API/Uploads`). Metadata in SQL Server. Bytes never in SQL Server. |

Dev URLs: site `http://localhost:4200`, API `http://localhost:5189`, health `http://localhost:5189/health`, SQL Server `localhost,1433`, database `cigt`.

## Layout

```text
frontend/src/app/     Angular app (auth, dashboard, clients, projects, users, customer-form, services)
API/Controllers/      HTTP only. Call a service. Return ApiResponse<T>.
API/Services/         Business rules.
API/Repositories/     EF Core queries.
API/Models/           Entities.
API/DTOs/             Request and response types.
API/Data/             ApplicationDbContext, DbSeeder, migrations live in API/Migrations/.
API/Configuration/    Options classes bound in Program.cs.
```

Frontend calls `environment.apiUrl` (`http://localhost:5189/api` in dev, `/api` in production). Do not hardcode hosts in components.

## How to add a feature

1. Entity or column change: edit the model, then `ApplicationDbContext.OnModelCreating`, then add an EF migration. Do not edit `ApplicationDbContextModelSnapshot.cs` by hand.
2. Repository method on the existing `I*Repository` if it is a query.
3. Service method. Return `ApiResponse<T>.Ok` or `Fail`. Do not throw for expected cases (not found, validation).
4. Controller action: `[Authorize]` unless it is the public customer form. Route under `api/...`. Pass `CancellationToken`.
5. Angular: model in `frontend/src/app/models/`, HTTP in a `providedIn: 'root'` service, standalone component, Angular Material, signals for UI state, Reactive Forms for input.
6. Register new DI in `API/Program.cs` next to the existing service registrations.

Migration command, from the repo root:

```bash
dotnet ef migrations add <Name> --project API --startup-project API
```

The API runs `Database.MigrateAsync` on startup. That is fine for local dev. Do not add a second migration runner.

## API shape

Match `ProjectController` and `ApiResponse<T>`:

- Success: `{ success, message, data }`
- Failure: `{ success: false, message, errors }`
- 200 with the wrapper on success, 400 on validation, 404 when the entity is missing
- DTOs in `API/DTOs/`. Do not return entities from controllers.
- Async all the way. `CancellationToken` on every public service and repository method.

New endpoints that change data stay `[Authorize]`. `CustomerFormController` GET/POST by token stay `[AllowAnonymous]`. Do not open other routes.

## Frontend shape

- Standalone components. Lazy routes in `frontend/src/app/app.routes.ts`.
- Logged-in pages sit under `MainLayoutComponent` with `authGuard`. Login and register sit under `AuthLayoutComponent` with `guestGuard`. `/users` also uses `adminGuard` and is hidden unless the signed-in user is an Admin.
- HTTP services return `Observable<ApiResponse<T>>`. The auth interceptor attaches the token. Do not attach tokens inside each service.
- Workspace drafts in `localStorage` are a client cache. The source of truth after save is SQL Server (`DocumentVersion`). Do not treat `localStorage` as the database.

## Files and images

This is the production rule. Do not break it.

- Store bytes through `IFileStorageService` only. `LocalFileStorageService` writes under `Storage:RootPath` (`Uploads` in dev).
- `Attachment` rows store file name, content type, size, relative `StoragePath`, uploader, and time. No `byte[]` column. No base64 column. No `varbinary(max)`.
- Upload path: `attachments/{projectId:N}/{attachmentId:N}_{safeFileName}`.
- Generated PDFs: `generated/{projectId:N}/latest.pdf` plus a timestamped copy. PDF generation must keep working if a listed file is missing (skip that file, do not throw away the whole document).
- Allowed uploads: PDF, DOCX, JPG, JPEG, PNG, and JSON. Reject anything else in `AttachmentFileRules` and in the file picker.
- PDF generation merges uploaded PDFs, DOCX files, and JPG/PNG images into the connector PDF. A DOCX is rendered to PDF pages and appended. JSON files are not merged. If at least one JSON file is on disk, the download is `CIGT_{Client}_{Application}.zip` containing `CIGT_{Client}_{Application}.pdf` plus those JSON files. With no JSON file, the download is the PDF under that same base name.
- A missing attachment file is skipped. Do not fail the whole PDF.
- Per-request cap today: 100 MB on `POST api/attachments/project/{id}/file`. Do not raise it without a reason.
- `UploadAsync` (JSON body, no bytes) is a legacy metadata-only path. Do not build new features on it. New uploads go through `UploadFileAsync`.
- Deleting an attachment deletes the file, then the row.
- Do not commit `API/Uploads/` or `API/App_Data/`.

To add cloud storage later: new class implementing `IFileStorageService`, register it in `RegisterFileStorage` in `Program.cs`, set `Storage:Provider`. Do not fork upload or PDF code per provider. Until that class exists, the only valid provider is `Local`. An unknown provider must keep failing startup.

## PDF

- `ConnectorPdfService.GenerateAsync` loads the project and attachments, resolves paths, calls `PdfGenerationService`, then `SaveGeneratedPdfAsync`. The saved file is always the PDF. The HTTP response is that PDF, or a zip with the same `CIGT_{Client}_{Application}` name when a JSON attachment exists. Share downloads that same zip when JSON files exist, and the PDF otherwise.
- `PdfGenerationService` builds the ARCON-branded body from `FormFieldCatalog`. New questionnaire fields must be added to `FormFieldCatalog` (section, key, label) or they will not appear in the PDF. A Jira ticket saved on the project is printed in the PDF header when it is set. It is not a questionnaire field.
- Share is `BuildShareComposeAsync`. `POST api/projects/{id}/share-email` returns the Outlook on the web recipient, subject, and default message. The recipient is `Email:ConnectorTeamAddress` (`ciconnectorsteam@arconnet.com`, display name CI Connectors Team). The browser opens `https://outlook.office.com/mail/deeplink/compose` and downloads the PDF or zip. That compose link cannot carry a file. The server does not send mail. Do not pretend SMTP exists. Wiring real mail is a new feature (no provider is configured).

## Auth and production safety

Do these by default on any production-bound change:

- Never commit connection strings, JWT keys, storage keys, or passwords. `appsettings.json` and `appsettings.Production.json` keep those empty. Local values belong in `appsettings.Development.json` or user secrets, not in new files.
- Do not copy the Docker `sa` password or the dev JWT key into production config.
- Do not print UAT passwords or tokens in logs.
- Roles stay Admin, Engineer, and Viewer. Do not widen that set.
- Public registration always assigns Engineer. Ignore any role sent on that request. Admins create the other roles from `/users` (`GET` and `POST api/users`). `UserAdminService` checks the Admin role and the controller returns 403 otherwise. Do not let public registration assign Admin again.
- New admin-only behavior needs an explicit role check. Most other controllers only use `[Authorize]`, so Viewer can still call them. A new destructive action should say which roles may call it.
- Email verification and forgot-password are placeholders (any non-empty code; reset token is not emailed). Do not claim they are finished. Do not log the reset token.
- `DbSeeder` creates roles, `admin@arconnet.com`, and sample clients when the client table is empty. Do not add more sample customers. Do not run destructive reseeds. If you touch the seeder, keep sample data from inserting into a database that already has real clients.
- Customer form tokens are secrets. Do not log them. Do not put a real customer token in a committed file.
- Questionnaire JSON (`CustomerForm.FormData`, `DocumentVersion.FormDataJson`) can contain UAT credentials in plain text. Do not add more secret fields to that JSON. Do not echo those values to the browser beyond the existing form.

## Database

- Provider: SQL Server. Connection name: `DefaultConnection`.
- Dev: `docker compose up -d` (container `cigt-sqlserver`, volume `cigt-sql-data`). Do not delete the volume.
- Tables you will touch: `Clients`, `Projects`, `CustomerForms`, `Attachments`, `ProjectDocuments`, `DocumentVersions`, `LookupItems`, `RefreshTokens`, plus ASP.NET Identity tables.
- `CustomerForm.FormData` and version JSON columns are `nvarchar(max)`. Keep them as JSON text. Do not replace them with a binary file column.
- Prefer a new migration over editing `20260716115635_InitialCreate`.
- Do not switch the app to MySQL. Do not add a second database.

## Commands

```bash
docker compose up -d
cd API && dotnet run --launch-profile http
cd frontend && npm start
```

Production API settings are environment variables (`ConnectionStrings__DefaultConnection`, `Jwt__Key`, `Storage__RootPath`, `Cors__AllowedOrigins__0`, `App__FrontendBaseUrl`, `ASPNETCORE_ENVIRONMENT=Production`). Do not hardcode a production host.

Frontend production build uses `frontend/src/environments/environment.production.ts` (`apiUrl: '/api'`). Change that only if the deployment design changes.

## Checks before you finish

- `dotnet build API/API.csproj` when you touched the API.
- Do not commit `bin/`, `obj/`, `node_modules/`, `frontend/dist/`, `Uploads/`.
- A UI change needs the affected flow exercised in the browser (login, the screen you changed, and any page that reads the same state).
- Commit only when the user asks. Messages: `feat:`, `fix:`, `refactor:`, `docs:`, `chore:`. Say why.

## Docs to keep true

When behavior, routes, roles, storage, auth, PDF, setup, or a production risk changes, update these in the same change:

- `CLAUDE.md` — agent instructions
- `SKILLS.md` — this file. If you add a skill under `.cursor/skills/`, mention it in `CLAUDE.md` and here
- `README.md` — features, status, setup, roadmap
- `DEMO.md` — startup and what a person clicks
- `docs/CIGT-Technical-Brief.md` — product shape and production risks

Do not add a new markdown file for the same facts. `frontend/README.md` is the Angular CLI template. Change it only when frontend run or build commands change.

The Cursor rule `.cursor/rules/keep-project-docs-current.mdc` repeats this so later sessions do it without being asked.

## Out of scope unless the user asks

Audit log, SMTP, Azure Blob implementation, role enforcement across every existing endpoint, Word export, swapping SQL Server. The Users page (list and create accounts) exists. A broader admin dashboard does not.
