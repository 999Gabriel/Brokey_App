# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Brokey** is a cross-platform expense-splitting app. Users create trips, organize expenses into groups, track who paid what, and view settlement calculations. It has multi-currency support with pre-seeded exchange rates.

## Solution Structure

```
Brokey_APP.sln
├── API_Server/     # ASP.NET Core 10 REST API (controllers, DTOs, JWT auth)
├── Brokey_APP/     # .NET MAUI 10 mobile client (Views, ViewModels, Services)
├── ORM/            # EF Core 10 data layer (AppDbContext, repositories, migrations)
└── Models/         # Shared domain entities used by both API and client
```

## Commands

```bash
# Build & restore
dotnet restore Brokey_APP.sln
dotnet build Brokey_APP.sln

# Run API (must use HTTPS profile — see auth notes below)
dotnet run --project API_Server/API_Server.csproj --launch-profile https
# API runs at https://localhost:7221, Swagger at http://localhost:5224/swagger

# Build MAUI client
dotnet build Brokey_APP/Brokey_APP.csproj -f net10.0-maccatalyst
dotnet build Brokey_APP/Brokey_APP.csproj -f net10.0-android
dotnet build Brokey_APP/Brokey_APP.csproj -f net10.0-ios

# EF Core migrations (always specify both projects)
dotnet ef migrations add <Name> --project ORM/ORM.csproj --startup-project API_Server/API_Server.csproj
dotnet ef database update --project ORM/ORM.csproj --startup-project API_Server/API_Server.csproj

# Tests (no test project exists yet — add xUnit + FluentAssertions when creating one)
dotnet test Brokey_APP.sln
```

## Architecture

### API Server
- **Controllers** → **Repositories** → **AppDbContext** (MySQL)
- DTOs live in `API_Server/DTOs/` and are separate from domain models in `Models/`
- `Program.cs` wires JWT auth, CORS, EF DbContext, Swagger
- User identity in protected endpoints is extracted via the `User.GetUserId()` extension method (reads JWT sub claim)
- CORS is AllowAll in development

### MAUI Client (MVVM)
- **Views** (`Views/*.xaml`) bind to **ViewModels** (`ViewModels/*.cs`) via `BindingContext`
- **ViewModels** inherit `BaseViewModel` (provides `IsBusy`, `Title`) and use `[ObservableProperty]` / `[RelayCommand]` from CommunityToolkit.Mvvm
- **Services** (`Services/`) are DI-injected into ViewModels: `AuthService`, `TripService`, `TokenStorageService`
- App routing is shell-based: unauthenticated → `AuthShell`, authenticated → `AppShell` (tab bar)
- All routes registered in `AppShell.xaml.cs`

### Data Layer (ORM)
- `AppDbContext` in `ORM/` manages all entities; repositories wrap per-aggregate queries
- MySQL backend, database name: `Brokey`
- Seeded data: 8 `ExpenseCategories` (Food, Night Out, Transport, etc.) and 25 `CountryCurrencies`
- Cascade deletes: Trip → Groups → Expenses; unique constraints on (GroupId, UserId) and (TripId, UserId)

### Authentication Flow
- `POST /api/auth/login` or `/register` → JWT issued by `TokenService`
- Client: `AuthService` saves token → `TokenStorageService` (memory → Preferences → SecureStorage fallback)
- `AuthHttpMessageHandler` injects `Authorization: Bearer <token>` on all non-auth requests
- On 401 response: token is cleared and user is routed back to login

## Critical: HTTPS-Only API Calls

**Always use HTTPS base URLs in `Brokey_APP/Services/ApiConfig.cs`.** If the client calls HTTP and the API redirects to HTTPS, the `Authorization` header is dropped, causing silent 401s on protected endpoints even after successful login. This was the root cause of the most significant auth bug in this project.

Android emulator uses `https://10.0.2.2:7221` instead of `localhost`.

## Coding Conventions

- C# with nullable reference types enabled (`<Nullable>enable</Nullable>`)
- `PascalCase` for types, methods, public properties; `camelCase` for locals/parameters; `_camelCase` for private fields
- MVVM boundary: no HTTP calls in Views, no UI navigation logic in Services
- Test naming: `MethodName_State_ExpectedResult`
- Commit format: `feat(scope): description` / `fix(scope): description`

## Current Feature Status

Working: register, login, logout, profile load, create/list trips, create groups, add/remove/list group members, navigation chain (dashboard → trips → trip detail → group detail).

In progress: expense creation and settlement display.