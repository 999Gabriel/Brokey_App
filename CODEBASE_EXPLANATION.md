# Brokey – Codebase Erklärung

Diese Datei erklärt die Architektur, den Datenfluss und alle wichtigen Komponenten der Brokey-App.

---

## Inhaltsverzeichnis

1. [Projektstruktur](#projektstruktur)
2. [Datenmodell (Models)](#datenmodell-models)
3. [Datenbankschicht (ORM)](#datenbankschicht-orm)
4. [API-Server](#api-server)
5. [MAUI-Client](#maui-client)
6. [Datenfluss: Vollständiges Beispiel](#datenfluss-vollständiges-beispiel)
7. [Authentifizierung](#authentifizierung)
8. [Ausgaben & Splits](#ausgaben--splits)
9. [Abrechnung (Settlement)](#abrechnung-settlement)
10. [Navigationsstruktur](#navigationsstruktur)

---

## Projektstruktur

```
Brokey_APP.sln
├── Models/          → Gemeinsame Domänen-Entitäten (C# Klassen, kein Verhalten)
├── ORM/             → Entity Framework Core: DbContext, Repositories, Migrations
├── API_Server/      → ASP.NET Core 10 REST-API (Controllers, DTOs, Services)
└── Brokey_APP/      → .NET MAUI 10 Mobile-App (Views, ViewModels, Services)
```

Die vier Projekte kommunizieren so:
- `API_Server` und `ORM` nutzen `Models` als gemeinsame Typen
- `Brokey_APP` (MAUI) spricht **nur** über HTTP mit dem `API_Server`
- `ORM` spricht über `AppDbContext` mit einer MySQL-Datenbank

---

## Datenmodell (Models)

Alle Klassen liegen im Projekt `Models/` und haben **keine Logik** – nur Properties und Navigation-Properties für EF Core.

| Klasse | Beschreibung | Wichtige Beziehungen |
|---|---|---|
| `User` | Registrierter Nutzer | hat viele `TripMemberships`, `GroupMemberships`, `PaidExpenses` |
| `Trip` | Reise mit Datum und Währung | hat viele `Groups`, `TripMembers`, `Expenses` |
| `TripMember` | Join-Tabelle User↔Trip | Rolle: "Owner" oder "Member" |
| `Group` | Untergruppe eines Trips (z.B. "Restaurants") | hat viele `GroupMembers`, `Expenses` |
| `GroupMember` | Join-Tabelle User↔Group | Rolle: "Admin" oder "Member" |
| `Expense` | Einzelne Ausgabe | gehört zu Trip + Group; hat viele `Splits` |
| `ExpenseSplit` | Anteil eines Users an einer Ausgabe | `IsSettled` + `SettledAt` für Abrechnung |
| `ExpenseCategory` | Kategorie (Food, Transport, …) | 8 vorgeseeddete Einträge |
| `ExchangeRate` | Wechselkurs zwischen zwei Währungen | für zukünftige Multi-Währungs-Umrechnung |
| `CountryCurrency` | Land → Währungscode | 25 vorgeseeddete Einträge |
| `Receipt` | Quittungs-Bild für eine Ausgabe | noch nicht im UI implementiert |

### Cascade-Regeln (wichtig!)

```
Trip       → (Cascade delete) → Groups → (Cascade) → GroupMembers
Trip       → (Cascade delete) → TripMembers
Trip       → (Cascade delete) → Expenses → (Cascade) → ExpenseSplits
Group      → (SetNull)        → Expenses.GroupId  (Ausgabe bleibt, verliert nur Gruppe)
```

---

## Datenbankschicht (ORM)

### AppDbContext (`ORM/AppDbContext.cs`)

Zentraler EF-Core-Datenbankkontext. `OnModelCreating` konfiguriert:
- Primärschlüssel, Indizes und Unique-Constraints
- Fremdschlüssel-Beziehungen mit Cascade-Regeln
- Dezimal-Genauigkeit (18,2 für Beträge; 10,7 für GPS-Koordinaten)
- Seed-Daten: 8 `ExpenseCategories` und 25 `CountryCurrencies`

### Repositories

Jedes Repository kapselt alle DB-Queries für eine Entität:

| Repository | Hauptaufgaben |
|---|---|
| `TripRepository` | CRUD für Trips; `GetTripsByUserAsync` filtert nach Mitgliedschaft |
| `GroupRepository` | CRUD für Groups; `GetByIdAsync` lädt Trip + Members |
| `GroupMemberRepository` | `AddMemberAsync` ist ein **Upsert** (fügt ein oder aktualisiert Rolle) |
| `TripMemberRepository` | `AddParticipantAsync` schützt Owner-Rolle vor Überschreibung |
| `ExpenseRepository` | Komplex: erstellt Splits, berechnet Aufteilungen, markiert Settlements |

#### ExpenseRepository im Detail

**`BuildExpenseProjectionQuery()`** – Wiederverwendbare Basis-Query mit allen nötigen `Include()`-Calls (Trip, Group, PaidBy, Category, Splits+User). Verhindert N+1-Queries.

**`BuildSplitEntities()`** – Erstellt `ExpenseSplit`-Objekte:
- Ohne `splitAmountsByUser` → gleichmäßige Aufteilung per `CalculateSplitAmounts`
- Mit `splitAmountsByUser` → direkte Zuweisung mit Rundungskorrektur

**`CalculateSplitAmounts()`** – Berechnet gleiche Anteile und hängt Cent-Reste an den ersten Teilnehmer: `10€ / 3 = [3,34, 3,33, 3,33]`.

---

## API-Server

### Startup (`Program.cs`)

Registriert in dieser Reihenfolge:
1. MySQL-Datenbankverbindung (via `AppDbContext`)
2. JWT-Bearer-Authentifizierung
3. Alle Repositories und `TokenService` als Scoped
4. CORS-Policy „AllowAll" (nur Development)
5. Middleware-Pipeline: HTTPS → CORS → Auth → Controllers

### Controllers

#### `AuthController` – `/api/auth`

| Endpoint | Methode | Beschreibung |
|---|---|---|
| `/register` | POST | Prüft Eindeutigkeit, hasht Passwort (BCrypt), erstellt User, gibt JWT zurück |
| `/login` | POST | Verifiziert BCrypt-Hash, gibt JWT zurück |
| `/me` | GET | Gibt Profil des eingeloggten Users zurück (JWT erforderlich) |

**`BuildAuthResponse()`** – Hilfsmethode, die aus einem `User`-Objekt + frischem JWT ein `AuthResponse`-DTO baut. Wird von Login und Register genutzt.

#### `TripsController` – `/api/trips`

| Endpoint | Methode | Beschreibung |
|---|---|---|
| `/` | GET | Alle Trips des Users (Ersteller oder Mitglied) |
| `/` | POST | Neuen Trip erstellen + Ersteller als Owner hinzufügen |
| `/{id}` | GET | Trip-Details mit Groups, Members, Expenses |
| `/{id}/groups` | GET | Alle Gruppen des Trips |
| `/{id}/groups` | POST | Neue Gruppe + Ersteller als Admin |
| `/recent-activities` | GET | 10 neueste Ausgaben über alle Trips des Users |

#### `GroupsController` – `/api/groups`

| Endpoint | Methode | Beschreibung |
|---|---|---|
| `/{id}/members` | GET/POST/DELETE | Mitglieder verwalten |
| `/{id}/expense-categories` | GET | Verfügbare Kategorien |
| `/{id}/expenses` | GET/POST | Ausgaben auflisten / erstellen |
| `/{id}/expenses/{eid}` | GET/PUT/DELETE | Einzelne Ausgabe lesen / bearbeiten / löschen |
| `/{id}/settlement` | GET | Abrechnungs-Übersicht (Balances + Transfers) |
| `/{id}/settlement/mark-settled` | POST | Schulden zwischen zwei Usern als bezahlt markieren |

**`BuildExpenseMutationContextAsync()`** – Zentrale Validierung für Create und Update: prüft Betrag, Titel, Gruppe, Kategorie, Zahler und Split-Logik. Gibt einen fertig vorbereiteten `ExpenseMutationContext` zurück.

**`GetAccessibleGroupAsync()`** – Sicherheits-Hilfsmethode, die in fast allen Endpoints aufgerufen wird. Prüft: Gruppe existiert + aufrufender User ist Trip-Mitglied.

### Services

**`TokenService`** – Erstellt JWTs mit Claims (Sub=UserId, Email, Username, Jti). Ablaufzeit und Schlüssel kommen aus `appsettings.json` → Sektion `Jwt`.

**`ClaimsPrincipalExtensions.GetUserId()`** – Extension-Methode auf `ClaimsPrincipal`. Liest UserId aus dem JWT-Claim. Alle Controller nutzen `User.GetUserId()`.

### DTOs vs. Domain-Modelle

| Schicht | Typ | Zweck |
|---|---|---|
| `Models/` | Domänenobjekte | EF-Core-Entitäten mit Navigation-Properties |
| `API_Server/DTOs/` | Request-DTOs | Eingehende JSON-Bodies (z.B. `CreateTripRequest`) |
| `API_Server/DTOs/` | Response-DTOs | Ausgehende JSON-Antworten (z.B. `TripDetailResponse`) |
| `Brokey_APP/Models/` | Client-Modelle | 1:1-Spiegelbilder der Server-Response-DTOs für den MAUI-Client |

---

## MAUI-Client

### Startup (`MauiProgram.cs`)

Registriert:
- `TokenStorageService` als **Singleton** (Token-Cache muss app-weit geteilt werden)
- Zwei `HttpClient`-Instanzen (für `AuthService` und `TripService`) mit:
  - `BaseAddress` aus `ApiConfig.BaseUri`
  - `AuthHttpMessageHandler` als DelegatingHandler
  - In DEBUG: SSL-Zertifikat-Validierung deaktiviert (für lokalen Entwicklungsserver)
- Alle ViewModels und Views als **Transient**

### Services

#### `TokenStorageService`

Dreistufiger Token-Speicher: **Memory → Preferences → SecureStorage**

```
SaveTokenAsync:   Memory ← Preferences ← SecureStorage (alle drei werden befüllt)
GetTokenAsync:    Memory → Preferences → SecureStorage (erste nicht-leere Quelle)
ClearTokenAsync:  alle drei werden geleert
```

Warum drei Stufen? Preferences funktioniert auf allen Plattformen zuverlässig; SecureStorage ist sicherer aber auf manchen Geräten nicht verfügbar.

#### `AuthHttpMessageHandler`

DelegatingHandler (HTTP-Middleware im Client). Fügt bei **jedem** Request automatisch den Bearer-Token ein – außer bei `/api/auth/login` und `/api/auth/register` (die brauchen noch keinen Token).

#### `AuthService`

Kommuniziert mit `/api/auth/*`. Speichert/löscht den Token über `ITokenStorageService`.

Wichtig: `LogoutAsync()` sendet **keinen** HTTP-Request – JWTs sind stateless, es genügt den Token lokal zu löschen.

#### `TripService`

Kommuniziert mit `/api/trips/*` und `/api/groups/*`. Alle Methoden rufen nach dem HTTP-Call `EnsureSuccessAsync()` auf, das bei 401 den Token löscht und bei anderen Fehlern die Server-Fehlermeldung extrahiert.

### MVVM-Pattern

```
View (XAML)  ←─BindingContext──→  ViewModel  ←─DI──→  Service  ─HTTP─→  API
     ↑                                ↑
  DataBinding                   [ObservableProperty]
  Commands                      [RelayCommand]
```

- **Views** sind passiv: nur XAML + minimaler Code-Behind
- **ViewModels** erben von `BaseViewModel` (IsBusy, Title via `[ObservableProperty]`)
- **Services** sind die einzige Stelle mit HTTP-Aufrufen

#### Query-Parameter-Navigation

Viele ViewModels implementieren `IQueryAttributable`:

```csharp
// Navigation zu einer Seite:
await Shell.Current.GoToAsync($"group-detail?groupId={group.Id}&groupName={Uri.EscapeDataString(group.Name)}");

// Empfang im ViewModel:
public void ApplyQueryAttributes(IDictionary<string, object> query)
{
    // Shell übergibt die Parameter hier
}
```

### ViewModels Übersicht

| ViewModel | Seite | Hauptaufgabe |
|---|---|---|
| `LoginViewModel` | LoginPage | Credentials eingeben, AuthService aufrufen, zu AppShell navigieren |
| `RegisterViewModel` | RegisterPage | Registrierungsformular validieren, User anlegen |
| `HomeViewModel` | HomePage | User-Begrüßung + letzte 10 Aktivitäten laden |
| `TripsViewModel` | TripsPage | Trip-Liste laden, zu Detail/Create navigieren |
| `CreateTripViewModel` | CreateTripPage | Formular für neuen Trip |
| `TripDetailViewModel` | TripDetailPage | Trip-Infos + Gruppen anzeigen, Gruppe erstellen |
| `TripSummaryViewModel` | TripSummaryPage | Zusammenfassung: Dauer, Kosten, Members, Gruppen |
| `GroupDetailViewModel` | GroupDetailPage | Members + Ausgaben + Settlement laden |
| `AddMemberViewModel` | AddMemberPage | Member per Username/Email zur Gruppe hinzufügen |
| `AddExpenseViewModel` | AddExpensePage | Ausgabe erstellen/bearbeiten (inkl. Split-Logik + GPS) |
| `ExpenseDetailViewModel` | ExpenseDetailPage | Einzelne Ausgabe anzeigen, bearbeiten, löschen |
| `ProfileViewModel` | ProfilePage | User-Profil anzeigen, Logout |
| `AboutViewModel` | AboutPage | App-Info, Link zu Impressum |

---

## Datenfluss: Vollständiges Beispiel

### "Ausgabe erstellen"

```
User tippt Betrag/Titel in AddExpensePage (XAML)
    ↓ DataBinding
AddExpenseViewModel.TitleText / AmountText
    ↓ SaveExpenseCommand
AddExpenseViewModel.SaveExpenseAsync()
    ↓ Validierung (Titel, Betrag, Kategorie, Split-Summe)
    ↓ Erstellt CreateExpenseRequest
TripService.CreateExpenseAsync(groupId, request)
    ↓ POST /api/groups/{groupId}/expenses + Bearer-Token
    ↓ (AuthHttpMessageHandler fügt Token ein)
GroupsController.CreateExpense()
    ↓ BuildExpenseMutationContextAsync() — Validierung + Split-Berechnung
ExpenseRepository.CreateGroupExpenseAsync()
    ↓ BuildSplitEntities() — ExpenseSplit-Objekte erstellen
    ↓ _context.Expenses.Add(expense) + SaveChangesAsync()
    ↓ GetGroupExpenseByIdAsync() — neu laden mit allen Includes
    ↓ MapExpense() — Expense → ExpenseResponse DTO
    ← HTTP 201 Created mit ExpenseResponse JSON
TripService.ReadRequiredAsync<ExpenseResponse>()
    ← ExpenseResponse Objekt
AddExpenseViewModel → Shell.GoToAsync("..") → zurück zur GroupDetailPage
```

---

## Authentifizierung

### Flow beim Login

```
1. LoginViewModel.LoginAsync()
2. AuthService.LoginAsync() → löscht alten Token → POST /api/auth/login
3. AuthController.Login() → BCrypt.Verify() → BuildAuthResponse() → TokenService.GenerateToken()
4. JWT zurück → AuthService speichert Token via TokenStorageService (Memory + Preferences + SecureStorage)
5. Application.Current.Windows[0].Page = new AppShell()  ← Shell wechselt
```

### JWT-Aufbau

```
Header: { "alg": "HS256" }
Payload: {
  "sub": "42",                    ← UserId (gelesen via User.GetUserId())
  "email": "user@example.com",
  "unique_name": "gabriel",
  "jti": "uuid",                  ← eindeutige Token-ID
  "exp": 1234567890               ← Ablaufzeit (aus Jwt:ExpireHours)
}
Signatur: HMACSHA256(base64(header+payload), Jwt:Key)
```

### Automatischer Logout bei 401

```
TripService.EnsureSuccessAsync() erkennt StatusCode 401
    → TokenStorageService.ClearTokenAsync()
    → wirft Exception("Your session is no longer valid. Please log in again.")
TripsViewModel fängt diese Exception
    → Application.Current.Windows[0].Page = new AuthShell()
```

---

## Ausgaben & Splits

### Split-Modi

| Modus | Wer kann ihn nutzen | Funktionsweise |
|---|---|---|
| `Equal` | Alle Mitglieder | Betrag wird gleichmäßig aufgeteilt; Cent-Rest geht an ersten Teilnehmer |
| `Percentage` | Nur Admin | Jeder Teilnehmer bekommt einen Prozentsatz; Summe muss 100% ergeben |
| `Amount` | Nur Admin | Jeder Teilnehmer bekommt einen fixen Betrag; Summe muss Gesamtbetrag ergeben |

### Split-Berechnung (Server)

```
GroupsController.BuildExpenseMutationContextAsync()
    ↓ NormalizeSplitMode()
    ↓ Bei Equal:    splitUserIds direkt übergeben
    ↓ Bei %/Amount: BuildCustomSplitAmounts() + EnsureExactAmountTotal()
ExpenseRepository.BuildSplitEntities()
    ↓ Bei Equal:    CalculateSplitAmounts(totalAmount, n)
    ↓ Bei Custom:   splitAmountsByUser direkt + Rundungskorrektur
→ List<ExpenseSplit> { UserId, Amount, IsSettled=false }
```

### Rundungskorrektur

Bei der Aufteilung entstehen oft Cent-Differenzen (z.B. 10€ / 3 = 3,333…). Die Lösung: alle Beträge auf 2 Dezimalstellen runden, den Rest (positiv oder negativ) zum **ersten Teilnehmer** addieren. Das garantiert, dass `SUM(splits) == expense.Amount`.

---

## Abrechnung (Settlement)

### Balance-Berechnung (`BuildBalances`)

Für jeden Gruppen-Member wird berechnet:

```
TotalPaid  = Summe aller Ausgaben, bei denen User als Zahler eingetragen ist
TotalShare = Summe aller eigenen Split-Beträge
NetBalance = TotalPaid - TotalShare
             (positiv = User hat mehr bezahlt als er schuldet = bekommt Geld zurück)
             (negativ = User schuldet noch Geld)
```

Bereits bezahlte Splits (IsSettled=true) werden herausgerechnet.

### Transfer-Berechnung (`BuildTransfers`)

Aggregiert alle offenen Splits zu einfachen "A schuldet B X€"-Einträgen:

```
Für jede Expense:
  Für jeden nicht-settled Split (wo User != Zahler):
    → { FromUserId: split.UserId, ToUserId: expense.PaidByUserId, Amount: split.Amount }
Gruppiert nach (From, To) und summiert
Gefiltert: Amount > 0.009 (ignoriert Cent-Beträge)
Sortiert: größter Betrag zuerst
```

### Settlement abschließen (`MarkSettlementAsSettled`)

Findet alle `ExpenseSplit`-Einträge wo:
- `split.UserId == fromUserId` (der Schuldner)
- `split.Expense.PaidByUserId == toUserId` (der Gläubiger)
- `split.IsSettled == false`
- `split.Expense.GroupId == groupId`

Und setzt `IsSettled = true`, `SettledAt = DateTime.UtcNow`.

---

## Navigationsstruktur

### Unauthentifiziert: `AuthShell`

```
AuthShell
├── //login    → LoginPage
└── //register → RegisterPage
```

### Authentifiziert: `AppShell` (Tab-Bar)

```
AppShell (Tab-Bar)
├── Tab: Home     → //home → HomePage
├── Tab: Trips    → //trips → TripsPage
├── Tab: Profile  → //profile → ProfilePage
└── Tab: About    → //about → AboutPage
    └── impressum → ImpressumPage

Modal/Stack-Routen (AppShell.xaml.cs registriert):
├── create-trip           → CreateTripPage
├── trip-detail           → TripDetailPage    (Query: tripId)
├── trip-summary          → TripSummaryPage   (Query: tripId)
├── group-detail          → GroupDetailPage   (Query: groupId, groupName)
├── add-member            → AddMemberPage     (Query: groupId, groupName)
├── add-expense           → AddExpensePage    (Query: groupId, groupName, [expenseId])
└── expense-detail        → ExpenseDetailPage (Query: groupId, expenseId)
```

### Shell-Wechsel

Der Wechsel zwischen `AuthShell` und `AppShell` erfolgt direkt über:
```csharp
Application.Current!.Windows[0].Page = new AppShell();  // nach Login/Register
Application.Current!.Windows[0].Page = new AuthShell(); // nach Logout / 401
```

---

## Bekannte Besonderheiten

### HTTPS-Pflicht im Client

`ApiConfig.cs` muss immer HTTPS-URLs enthalten. Bei HTTP-Requests leitet der Server zu HTTPS um, dabei geht der `Authorization`-Header verloren → stille 401-Fehler. Das war der größte Auth-Bug in der Projekt-Historie.

### Android-Emulator

Verwendet `https://10.0.2.2:7221` statt `localhost`, weil der Emulator den Host-Rechner unter dieser IP erreicht.

### Token-Speicherung

`SecureStorage` ist auf einigen Android-Geräten/Emulatoren nicht verfügbar. Deshalb wird immer in `Preferences` als Fallback gespeichert. Der In-Memory-Cache (`_cachedToken`) vermeidet wiederholte asynchrone Lese-Operationen.
