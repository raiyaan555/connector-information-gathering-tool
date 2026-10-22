# How to start the CIGT demo

This is the Connector Information Gathering Tool. It has three parts that must all be running:

1. **Database** — stores users, clients, and projects (SQL Server in Docker)
2. **API** — the backend (http://localhost:5189)
3. **Website** — the screen you click in the browser (http://localhost:4200)

You need **three terminal windows**. Keep all three open while you show the demo.

## Before you start (one time)

Install these if they are not already on the Mac:

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20 or newer](https://nodejs.org/)

Open **Docker Desktop** and wait until it says it is running. The whale icon in the menu bar should be steady, not animated.

The first time only, install frontend packages:

```bash
cd frontend
npm install
```

## Every time you want to show the demo

Open the project folder in each terminal:

```bash
cd "/Users/raiyaan/Downloads/ARCON WORK/Connector Information Gathering Document Project"
```

### Terminal 1 — start the database

```bash
docker compose up -d
```

Wait until it finishes. You should see the container name `cigt-sqlserver`. Data stays saved even if you close Docker later, because it uses a named volume.

Check that it is up:

```bash
docker ps
```

You should see `cigt-sqlserver` with status `Up`.

If that command says **port 1433 is already allocated**, a SQL Server container is already running (often named `sqlserver`). That is fine. Leave it running and go to Terminal 2. The API uses `localhost` on port `1433` with password `StrongPassword@123`, so it can talk to the database that is already there.

### Terminal 2 — start the API

```bash
cd API
dotnet run --launch-profile http
```

Wait until you see a line like:

```text
Now listening on: http://localhost:5189
```

The first start can take a minute. It creates the database tables and a sample admin account by itself. Leave this terminal running.

Quick check in a browser: http://localhost:5189/health

### Terminal 3 — start the website

```bash
cd frontend
npm start
```

Wait until you see:

```text
Local: http://localhost:4200/
```

Leave this terminal running.

### Open the app

Go to **http://localhost:4200**

Log in with:

| Field | Value |
|-------|--------|
| Email | `admin@arconnet.com` |
| Password | `Password123` |

## What to click in the demo

1. After login you land on the **dashboard**. Sample clients and applications are already there (GitLab, Salesforce, Workday, and others).
2. Use the search bar at the top to find a client or application.
3. Click **Add Client** to create a new customer container.
4. On a client card, click **Add Application** to start a new connector project.
5. Open an existing application to see the document workspace. From there you can review details and use **Download PDF**.
6. The customer form is a public page (no login). One sample link is already seeded:

   http://localhost:4200/form/a1b2c3d4-e5f6-7890-abcd-ef1234567890

7. Click **Logout** in the top right when you are done.

New company accounts must use an email that ends with `@arconnet.com`. Passwords need at least 8 characters, one uppercase letter, one lowercase letter, and one number.

## How to stop

1. In the website terminal, press `Ctrl + C`.
2. In the API terminal, press `Ctrl + C`.
3. Stop the database when you are finished for the day:

```bash
docker compose stop
```

Use `stop`, not a command that deletes the container. Stopping keeps the saved data.

## If something does not open

| What you see | What to do |
|--------------|------------|
| Website says it cannot reach the API | Make sure Terminal 2 is still running and http://localhost:5189/health opens |
| API fails on startup with a database error | Open Docker Desktop, then run `docker compose up -d` again and wait about 20 seconds |
| Port already in use | Another copy is already running. Close the old terminal or stop that process, then start again |
| Login fails | Use `admin@arconnet.com` and `Password123`. The API must have finished its first start so the admin user exists |
| Blank page on port 4200 | Wait until `npm start` prints `Local: http://localhost:4200/` |

## Addresses to remember

| Piece | Address |
|-------|---------|
| Website (demo this) | http://localhost:4200 |
| API | http://localhost:5189 |
| API health check | http://localhost:5189/health |
| Sample customer form | http://localhost:4200/form/a1b2c3d4-e5f6-7890-abcd-ef1234567890 |
