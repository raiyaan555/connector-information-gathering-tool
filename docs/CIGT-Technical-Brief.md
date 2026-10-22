# Connector Information Gathering Tool (CIGT)

**Technical brief for leadership**

| | |
|---|---|
| Product | Connector Information Gathering Tool (CIGT) |
| Owner | ARCON — Implementation |
| Status | Working application, suitable for internal demo. Not production-ready without the items in section 7. |
| Audience | Engineering manager and CTO |
| Date | 23 October 2026 |
| Printable pack | [CIGT-CTO-Technical-Review.pdf](CIGT-CTO-Technical-Review.pdf) — stack, folders, use cases, and the decisions to make before a larger UML exercise |

---

## 1. What this application is

CIGT replaces the Word document that Implementation Engineers fill in when ARCON builds a connector for a customer application.

Today that work is a manual document: purpose of the application, how we connect to it, lifecycle (create / modify / delete users), SSO, source of truth, encryption, API notes, screenshots, and supporting PDFs. Engineers then email that document to the connector team.

CIGT turns that into a web application:

1. An engineer signs in with an `@arconnet.com` account.
2. They create a **client** (the customer) and an **application / project** under that client (GitLab, Workday, SAP, and so on).
3. They fill a structured connector questionnaire, save drafts, and attach supporting files (PDF, DOCX, JPG, PNG, or JSON only).
4. A customer can also fill a public form via a unique link, with no ARCON login.
5. The system generates a branded PDF that includes the answers. Uploaded PDFs, DOCX files, and images are merged into it. JSON files stay as JSON.
6. The engineer downloads that PDF. If a JSON file was stored, the download is a zip named `CIGT_{Client}_{Application}` containing the PDF and the JSON files. **Share to Connector Team** opens Outlook on the web, addressed to the CI Connectors Team group (`ciconnectorsteam@arconnet.com`), with a message the engineer can edit. The same PDF or zip downloads so they can drop it onto that message. Outlook's compose link cannot carry the file. A Jira ticket can be saved on the application; it is included in the message and printed on the PDF.
7. Later edits are kept as a new document version, with the previous version still available.

Word and PowerPoint export were removed on purpose. The document the tool creates is a PDF. JSON attachments are delivered beside that PDF in a zip.

---

## 2. Who uses it

| User | How they get in | What they do |
|---|---|---|
| Implementation Engineer | Login. Public registration always assigns this role. | Clients, projects, questionnaire, files, PDF, share. |
| Admin | Seeded account, or created by an existing Admin on the Users page. | Same product screens, plus **Users** (`/users`): list accounts and create Engineer, Viewer, or Admin. |
| Viewer | Created by an Admin on the Users page. | Same product screens today. The role does not yet block edit or delete. |
| Customer | Public link, no login. | Fills the requirement form for one project. |

Roles are stored (Admin, Engineer, Viewer). Public registration ignores any role on the request and always creates an Engineer. `GET` and `POST api/users` require Admin and return 403 otherwise. Other APIs still only check “is this person logged in?” Viewer cannot yet be stopped from editing or deleting. That remaining role enforcement is a go-live item.

---

## 3. How the system is built

Three pieces run together. None of them is optional.

```text
Browser (Angular)
        |
        |  HTTPS in production, HTTP on localhost today
        v
ASP.NET Core Web API
        |
        +---- SQL Server          users, clients, projects, answers, file metadata
        |
        +---- File folder         the actual PDFs and images
              Uploads/
```

| Layer | What we use | Why |
|---|---|---|
| Website | Angular 20, TypeScript, Angular Material, SCSS | Internal UI. Forms, dashboard, project workspace. |
| UI state | Angular Signals, Reactive Forms | Form state stays in the page without a separate state library. |
| API | ASP.NET Core 10 (C#) | REST API the website calls. |
| Auth | ASP.NET Core Identity, JWT access tokens, refresh tokens | Company login. Access token lasts 60 minutes in the production config (120 in development). Refresh token lasts 7 days. |
| Data access | Entity Framework Core 10 | Tables, migrations, and queries. Migrations run automatically when the API starts. |
| Database | Microsoft SQL Server 2022 | Development runs SQL Server in Docker. Production is expected to be a real SQL Server instance. Database name: `cigt`. |
| PDF | QuestPDF (document layout) and PdfSharpCore (merge uploaded PDFs and place images on pages) | One branded PDF per project. |
| Files | Local disk today, behind an interface (`IFileStorageService`) | Bytes are not stored inside SQL Server. See section 5. |
| Email share | Outlook on the web, opened in the browser and addressed to the CI Connectors Team group. The engineer edits the message, drops on the downloaded PDF or zip, and presses Send. | The server does not send mail. SMTP is not connected. The compose link cannot attach a file. |
| Health | `GET /health` | Confirms the API can reach SQL Server. |

Development addresses:

| Piece | Address |
|---|---|
| Website | http://localhost:4200 |
| API | http://localhost:5189 |
| Health check | http://localhost:5189/health |
| SQL Server | localhost, port 1433, database `cigt` |

The production website is built to call the API at `/api` on the same domain (for example behind IIS or a reverse proxy). Production settings for the API URL, database, JWT signing key, and upload folder are environment variables. They are empty placeholders in the production config file on purpose, so secrets are not committed.

---

## 4. What is stored in the database

SQL Server holds structured data only. A simplified map:

```text
Users and roles          ASP.NET Identity tables + RefreshTokens
        |
Clients                  company, contact, industry, country
        |
        +-- Projects     application name, status, priority, engineer, public form token
                |
                +-- CustomerForms       customer answers (JSON text)
                +-- Attachments         file name, type, size, path, who uploaded it
                +-- ProjectDocument
                        |
                        +-- DocumentVersions   each save: answers, attachment list, completion %
```

Lookup values (project status, priority, application type, role names) are seeded into `LookupItems`.

Project status values: Draft, In Progress, Pending Review, Completed.

Application types used by the questionnaire: LDAP, database, API, SDK, RPA.

### What is deliberately not in the database

The PDF bytes, screenshots, and uploaded documents are **not** columns in SQL Server. The `Attachments` row stores:

- file name
- content type (`application/pdf`, `image/png`, `image/jpeg`)
- size in bytes
- relative path on disk (`attachments/{projectId}/{id}_{fileName}`)
- who uploaded it and when

Generated PDFs are written under `generated/{projectId}/` as `latest.pdf` plus a timestamped copy. The database does not keep a binary copy of those either.

Customer answers and each document version are stored as JSON text (`nvarchar(max)`). That includes long written answers. It also includes fields the questionnaire asks for, such as a UAT username and UAT password. Those values are plain text in SQL Server today. That is acceptable for a demo and not acceptable for a customer production system without a decision on encryption and access control (section 7).

---

## 5. Files and images — the question that matters for production

### What we do today

When an engineer uploads a file:

1. The API accepts it (maximum about 100 MB per request).
2. The bytes are written to a folder on the machine running the API.
3. A row is inserted in SQL Server with the path and metadata.
4. When a PDF is generated, the API reads those files from disk. Uploaded PDFs are appended. PNG and JPG images are drawn on their own pages. Other file types are stored but are not merged into the PDF.

This split is the right design. The database stays small and fast. Backups of SQL Server stay backups of data, not of every screenshot.

Development folder: `API/Uploads/` next to the API (not committed to git).

The production config currently points at a single Windows folder:

`C:\inetpub\cigt\Uploads`

That only works if production is one Windows server, that folder is on a disk that survives restarts, and that disk is backed up separately from the database.

### What goes wrong if we put the files inside SQL Server

Putting images and PDFs in the database (a `varbinary(max)` column, or base64 text) is the approach to avoid.

| Problem | What happens in production |
|---|---|
| Database size | A year of connector packs, screenshots, and generated PDFs turns a small database into tens or hundreds of gigabytes. |
| Backups and restores | Every nightly backup copies every file again. Restore time goes from minutes to hours. Point-in-time restore becomes expensive. |
| Performance | SQL Server reads those blobs into memory with the row. Listing a project’s files, or opening a project, gets slower as attachments accumulate. |
| Transactions | A 50 MB upload is wrapped in a database transaction. The transaction log grows. A failed upload can bloat the log. |
| Scaling the API | Two API servers can share one database. They cannot share a blob that only one of them wrote into temp space efficiently, and large reads compete with normal queries. |
| PDF generation | Generation already reads the file from disk and merges it. Pulling the same bytes out of SQL first adds load for no benefit. |
| Deletion | Deleting a project would delete huge binary rows inside the same transaction as the project row. |

SQL Server *can* store files. For this product it is the wrong place. Metadata stays in SQL Server. Bytes stay in a file store.

### What we should do when this goes to production

The code already has a storage interface. Today only `Local` is implemented. Any other provider name stops the API on startup, so we cannot accidentally point production at a cloud setting that does not exist yet.

Recommended production design:

1. **Keep the `Attachments` table as it is.** Add a storage provider name on the row if we need to migrate old files later. Do not add a binary column.
2. **Put bytes in Azure Blob Storage** (or an equivalent object store / secured file share if ARCON standard is on-premises). One container, path still `attachments/{projectId}/{id}_{fileName}` and `generated/{projectId}/...`.
3. **Implement that as a second `IFileStorageService`.** Switch `Storage:Provider` from `Local` to the new provider. The upload, delete, and PDF code already call that interface. They should not need a redesign.
4. **Give the API a managed identity or a key vault secret** for the storage account. Do not put a storage key in `appsettings`.
5. **Back up the blob container** on its own schedule. A SQL backup without the files means the PDF generator will skip every attachment whose path no longer exists. That failure is silent today: a missing file is skipped, and the PDF is still produced.
6. **Keep the upload allow-list** (PDF, DOCX, JPG, JPEG, PNG, and JSON) and consider a lower default size (for example 25 MB) unless a project truly needs 100 MB. Scan files if ARCON’s security standard requires it.
7. **If we must stay on one Windows server for the first release**, use the local folder only as a temporary measure: put it on a persistent disk, include it in backup, and do not run more than one API instance. Document that a second server will not see the files. Plan the blob move before a second instance or a container deployment.

Local disk is correct for a developer laptop. It is not correct for more than one server, for containers that are replaced on each deploy, or for any environment where the disk is wiped.

---

## 6. End-to-end flow (for the demo and for production)

```text
Engineer logs in
        |
        v
Dashboard  -->  search clients and applications
        |
        v
Add client  -->  company record in SQL Server
        |
        v
Add application  -->  project row + public form token
        |
        +--> Engineer fills the workspace (draft in the browser until saved)
        |
        +--> Customer opens /form/{token} and submits (no login)
        |
        v
Save  -->  a DocumentVersion row (answers + attachment list)
        |
        v
Upload PDF / DOCX / JPG / PNG / JSON  -->  file on disk, metadata in Attachments
        |
        v
Generate PDF  -->  QuestPDF body + merged PDF, DOCX, and images
        |
        +--> Download PDF, or CIGT_Client_Application.zip when a JSON file exists
        |
        +--> Share  -->  Outlook on the web opens to CI Connectors Team
                         message is editable; PDF or CIGT zip downloads
                         engineer drops the file on the message and sends
                         (the server does not send the email)
```

Drafts typed in the workspace are also kept in the browser (`localStorage`) until they are saved. If an engineer changes laptops before saving, that unsaved draft is only on the first machine.

---

## 7. What will break or become a risk in production

These are the items to close before real customer data is entered. They are ordered by how much they matter.

### Must fix before go-live

| Risk | What happens if we ignore it | What to do |
|---|---|---|
| **Files live on one server’s disk** | A redeploy, a second API instance, or a new VM loses every screenshot and PDF. The database still lists the files. PDF generation silently omits them. | Blob storage, or one persistent backed-up disk and a single API instance. See section 5. |
| **Demo data and a known admin password are created on startup** | If the client table is empty, startup inserts sample banks and projects and creates `admin@arconnet.com` with a published demo password. | Seed roles only in production. Create the first admin out of band. Remove sample clients. Rotate any password that has appeared in docs or demos. |
| **Secrets are not in a vault yet** | Database password and JWT signing key must be supplied at runtime. If the development key or the Docker SQL password is reused, anyone who has seen the repo or the demo can forge tokens or read the database. | Production connection string and `Jwt:Key` from a secret store. Different key from development. Minimum 32 characters, actually long and random. |
| **Anyone at ARCON can still self-register as Engineer** | Public registration no longer accepts a role. It always creates an Engineer, and only an Admin can create another Admin from the Users page. Open registration itself is still on: any `@arconnet.com` address can create an Engineer account. | Turn off open registration if IT will provision users. Keep Admin creation on the Users page. |
| **Roles are only enforced on the Users page** | Viewer, Engineer, and Admin can still call the same client, project, upload, and PDF APIs. Admin is required only for `api/users`. | Add role checks on create, edit, delete, and PDF share before external or broader internal use. |
| **Email verification does not verify anything** | “Verify email” marks the account confirmed for any non-empty code. Forgot-password generates a reset token and does not send it. Login does not require a confirmed email. | Connect SMTP or Microsoft 365. Send real verification and reset links. Reject login until the address is confirmed, or disable both screens until mail works. |
| **UAT passwords are stored as plain text** | The questionnaire asks for a UAT username and password. Those strings are saved in JSON in `CustomerForms` and `DocumentVersions`, and they are printed into the PDF. | Confirm with security whether UAT secrets belong in this tool. If they do, encrypt that field at rest and restrict who can open the project. If they do not, remove the field and keep credentials in the existing secrets process. |
| **Customer form is public** | Anyone with the link can read and submit that project’s form. The token is an unguessable id, not a login. | Treat the link as a secret. Add expiry, and ideally a one-time or customer-specific check, before links go to customers. |
| **No HTTPS in the app itself** | Development is HTTP on localhost. Tokens in the browser are exposed if production is also plain HTTP. | Terminate TLS on IIS, a reverse proxy, or Azure. Set the production site URL and CORS to that HTTPS origin only. |
| **QuestPDF license** | The PDF library is running under the QuestPDF Community license. That license is for smaller organisations. ARCON will need the commercial license confirmed with QuestPDF before production use. | Procurement confirms the license and we set the production license type. PdfSharpCore remains the merger for uploaded PDFs and images. |

### Will hurt once more than one person uses it seriously

| Risk | Detail |
|---|---|
| **Upload rules are still thin** | PDF, DOCX, JPG, PNG, and JSON are the only types accepted, and the file contents are checked. The size cap is still 100 MB. A metadata-only upload endpoint still exists and can create a database row with no file on disk. There is no malware scan. |
| **No malware scan** | Customer and engineer uploads land on the server as-is. |
| **Generated PDFs accumulate** | Every generation writes `latest.pdf` and another full timestamped copy. Disk use grows even when the user only keeps the latest. |
| **Share is not really email** | Share opens Outlook on the web. The engineer still has to drop the downloaded PDF or zip onto the message and press Send. There is no record that the connector team received it. A pop-up blocker can stop the Outlook tab. |
| **No audit log** | We do not record who changed a client, who downloaded a PDF, or who deleted a file. That will be asked for the first time something is disputed. |
| **Browser drafts** | Unsaved answers live in that browser only. Clearing site data or switching computers drops them. |
| **Single database, migrations on startup** | The API applies schema changes when it starts. That is convenient for a demo. In production, a bad migration takes the API down, and two instances starting together can race. Run migrations as a release step, then start the API. |
| **SQL Server in Docker is the dev database only** | The Docker password is a development password. Production needs a managed SQL Server (or Azure SQL), a least-privilege user (not `sa`), encrypted connections, backups, and a restore test. |
| **No automated test suite that covers the business flow** | The frontend test setup is the Angular default. There is no API test project covering login, upload, or PDF generation. Regressions will show up in demos. |
| **Sample and real data can mix** | Sample clients are inserted only when the client table is empty. If someone clears clients in production, the next restart inserts the sample banks again. |
| **Logging** | Errors are logged. There is no central log store, no correlation id for a request, and no alert if the disk fills or PDF generation starts failing. |
| **CORS and allowed hosts** | Production config still says `https://your-production-domain.com` and allows any host header. Both must be set to the real site before go-live. |

### Scaling picture (so expectations are clear)

This is an internal tool for Implementation, not a high-traffic public product. One API instance and one SQL Server are enough for the team that will use it first.

It will not scale horizontally until files move off local disk. It does not need a cache, a message queue, or Kubernetes for the first production release. Those would add operations work without fixing the risks above.

A practical first production shape:

```text
Browser  -->  HTTPS reverse proxy / IIS
                    |
                    +-- Angular static files
                    +-- ASP.NET Core API (one instance)
                              |
                              +-- Azure SQL or SQL Server (backups on)
                              +-- Azure Blob (or one persistent upload disk)
```

---

## 8. What is done, and what is not

| Area | State |
|---|---|
| Login, refresh tokens, lockout after 5 failures (15 minutes) | Done |
| Register, profile | Done. Public registration is always Engineer. Mail and real verification are not. |
| Users page (Admin lists and creates accounts) | Done. Not a full admin console. |
| Dashboard, clients, projects | Done |
| Connector questionnaire, review, version history | Done |
| PDF with ARCON branding, uploaded PDFs merged, images on their own pages | Done |
| Share opens Outlook on the web to the CI Connectors Team, with an editable message. The PDF or zip downloads for the engineer to attach. A Jira ticket can be saved on the application. | Done. Server-side send is not. |
| SQL Server schema and automatic migration | Done for development |
| File metadata in SQL, bytes on disk | Done |
| Health check | Done |
| Cloud file storage | Not built. Interface is ready for it. |
| SMTP email | Not built |
| Audit log | Not built |
| Role checks on each action | Users API only. Other actions still any signed-in user. |
| Admin console | Users page only. No broader admin dashboard. |
| Word / PowerPoint export | Removed on purpose |

---

## 9. Decisions needed from leadership

1. **Where production runs.** One Windows server (IIS) for the first release, or Azure (App Service + Azure SQL + Blob). The file design depends on this.
2. **Where files live.** Approve blob storage (recommended) or accept a single-server disk with a written limit of one API instance.
3. **Whether UAT passwords belong in this tool.** If yes, we encrypt them. If no, we remove those fields before any real project is saved.
4. **Who may have an account.** Open Engineer registration for `@arconnet.com` is still on. Admins can already create other roles from the Users page. Decide whether IT should provision every account instead.
5. **QuestPDF commercial license.** Confirm before the first production PDF is sent to a customer.
6. **Email.** Opening Outlook on the web is the v1 share. The engineer still attaches the downloaded file and presses Send. Does the connector team need the server to send the mail and record that it was sent?

---

## 10. Suggested order of work before production

1. Turn off sample data and the demo admin in the production environment. Secrets from a vault. HTTPS and the real site URL.
2. Decide whether open Engineer registration stays on. Enforce roles on edit, delete, and PDF share. Admin self-registration is already closed.
3. Move files to blob storage (or formally accept one disk and one server).
4. Allow only PDF and images, and stop creating attachment rows that have no file.
5. Decide on UAT credentials. Remove or encrypt.
6. Real email for password reset, or hide those screens.
7. Audit log for upload, delete, PDF download, and project changes.
8. Run migrations as a deployment step. Add a backup and a restore drill for both the database and the files.

None of this requires rewriting the application. The product shape — Angular, ASP.NET Core, SQL Server, PDF, files beside the database — is the one we should keep.
