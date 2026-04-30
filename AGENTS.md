# Repository Guidelines

## Project Structure & Module Organization
- `Brokey_APP.sln`: solution entry point.
- `API_Server/`: ASP.NET Core Web API (controllers, DTOs, auth, startup config).
- `Brokey_APP/`: .NET MAUI client app (Views, ViewModels, Services, platform-specific files).
- `ORM/`: EF Core data layer (`AppDbContext`, repositories, migrations).
- `Models/`: shared domain models used by API and client.
- `Brokey_APP/Resources/`: fonts, images, styles, splash/icon assets.

## Build, Test, and Development Commands
- Restore/build whole solution:
  - `dotnet restore Brokey_APP.sln`
  - `dotnet build Brokey_APP.sln`
- Run API locally (HTTPS profile):
  - `dotnet run --project API_Server/API_Server.csproj --launch-profile https`
- Build MAUI app (example MacCatalyst target):
  - `dotnet build Brokey_APP/Brokey_APP.csproj -f net10.0-maccatalyst`
- EF Core migration workflow:
  - `dotnet ef migrations add <Name> --project ORM/ORM.csproj --startup-project API_Server/API_Server.csproj`
  - `dotnet ef database update --project ORM/ORM.csproj --startup-project API_Server/API_Server.csproj`
- Tests:
  - `dotnet test Brokey_APP.sln` (currently no dedicated test project; add one before relying on CI quality gates).

## Coding Style & Naming Conventions
- Language: C# with nullable reference types enabled.
- Use 4-space indentation and standard .NET formatting.
- Naming:
  - `PascalCase` for types, methods, and public properties.
  - `camelCase` for locals/parameters.
  - Private fields: `_camelCase`.
- Keep MAUI MVVM boundaries clear:
  - UI logic in `ViewModels/`, transport/auth in `Services/`, UI markup in `Views/`.

## Testing Guidelines
- Preferred stack for new tests: xUnit + FluentAssertions (or existing team choice).
- Organize tests by project area (e.g., `API_Server.Tests/Controllers/...`).
- Naming convention: `MethodName_State_ExpectedResult`.
- Focus first on auth flows, trip/group endpoints, and repository behavior.

## Commit & Pull Request Guidelines
- Follow concise, structured commit messages, e.g.:
  - `feat(auth): add token refresh handling`
  - `fix(trips): use https base url to avoid 401 on redirect`
- Keep commits scoped to one concern.
- PRs should include:
  - What changed and why.
  - How to test (`dotnet` commands and endpoints/screens).
  - Migration notes if schema changed.
  - Screenshots/GIFs for MAUI UI changes.

## Security & Configuration Tips
- Do not commit secrets in `appsettings*.json`.
- Keep API base URL HTTPS for authenticated calls.
- Validate JWT-related changes with both `login/register` and protected endpoints (`/api/trips`, `/api/groups/...`).
