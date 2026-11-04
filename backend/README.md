# RestaFlow Backend

REST API for RestaFlow. It serves the Nuxt web interface in the repository root and shares its data layer with the desktop client.

## Structure

| Project | Target | Purpose |
| --- | --- | --- |
| `RestaFlowApi` | ASP.NET Core Web API (.NET 8) | Controllers for accounts, orders, products and tables, with Swagger |
| `RestaFlowSharedLib` | .NET Framework 4.7.2 class library | Models and Dapper repositories over SQL Server |

Open `RestaFlowBackend.sln` in Visual Studio 2022. NuGet packages for `RestaFlowSharedLib` restore into `backend/packages` (git-ignored).

## Configuration

The database connection string is **not** stored in the repository. The shared data layer (`DBContext`) reads it from the `RESTAFLOW_DB_CONNECTION` environment variable and throws a clear error if it is missing.

PowerShell (current user, restart Visual Studio afterwards):

```powershell
setx RESTAFLOW_DB_CONNECTION "Server=localhost;Database=RestoranTakip;User Id=<sql-login>;Password=<password>;TrustServerCertificate=True;"
```

Use a dedicated SQL login with only the permissions this application needs, never `sa`. Do not put the value in `appsettings.json` or `launchSettings.json`; both are committed.

## Running

- The API listens on `https://localhost:7253` and serves Swagger at the root path. Trust the development certificate once with `dotnet dev-certs https --trust`.
- CORS allows the Nuxt dev server at `http://localhost:3000`.
- The database schema is not included in this repository yet.

## Security notes

This is a learning/portfolio project and is not production-ready:

- Account passwords are currently stored and compared in plain text. Hash them (for example with ASP.NET Core Identity's `PasswordHasher`) before using real accounts.
- Endpoints have no authentication or authorization yet.
- Swagger is enabled in every environment.
