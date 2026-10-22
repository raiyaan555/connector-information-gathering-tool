---
name: cigt-development
description: >-
  Develop the Connector Information Gathering Tool (CIGT): Angular 20 frontend,
  ASP.NET Core 10 API, SQL Server, PDF generation, and file uploads. Use when
  editing this repo, adding API or Angular features, changing the database,
  attachments, images, PDFs, auth, or preparing production behavior.
---

# CIGT development

Before writing code, read [CLAUDE.md](../../../CLAUDE.md) and [SKILLS.md](../../../SKILLS.md) at the repo root and follow them. If behavior changes, update those files and the other project docs listed in `CLAUDE.md` in the same change.

Hard rules if you have not opened that file yet:

- Database is SQL Server via EF Core. Do not introduce MySQL or any other engine.
- File bytes go through `IFileStorageService` onto disk (`API/Uploads`). SQL Server stores attachment metadata only. Never add a `byte[]`, base64, or `varbinary` column for PDFs or images.
- PDF only. Do not restore Word or PowerPoint export.
- New HTTP features follow Controller → Service → Repository and return `ApiResponse<T>`.
- Public registration always assigns Engineer. Only an Admin may create other roles, from the Users page.
- Do not commit secrets, `Uploads/`, `bin/`, `obj/`, or `node_modules/`.
- Do not seed extra sample clients. Do not log reset tokens, form tokens, or UAT passwords.
