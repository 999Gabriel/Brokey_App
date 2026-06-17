# 🎤 Brokey – Spickzettel für die Präsentation

> **Was ist Brokey?** Eine App zum Aufteilen von Reisekosten. User legen Reisen an, organisieren Ausgaben in Gruppen, erfassen wer was bezahlt hat, und sehen die Abrechnung (wer schuldet wem). Mit Mehrwährungs-Unterstützung.

> 💡 **Tipp:** Das interaktive Diagramm `Brokey_Architektur.html` (Doppelklick) zeigt alle Datenflüsse zum Vorführen. Szenario wählen → ▶ Abspielen.

---

## 1️⃣ Das große Ganze – 4 Projekte

Stell es dir wie ein Restaurant vor:

| Projekt | Rolle | Was es ist |
|---|---|---|
| **Brokey_APP** | Der Gastraum (was der Gast sieht) | MAUI-App fürs Handy – Bildschirme & Buttons |
| **API_Server** | Der Kellner | ASP.NET Web-API – nimmt Anfragen entgegen, liefert Daten |
| **ORM** | Die Küche | EF Core – redet mit der Datenbank |
| **Models** | Die Rezeptkarten (alle teilen sie) | Einfache C#-Klassen wie `User`, `Trip` |

**Der wichtigste Satz:** Die Handy-App redet **nie** direkt mit der Datenbank. Sie fragt immer den API-Server (über HTTPS), und der Server fragt die Datenbank.

```
Brokey_APP  ──HTTPS──▶  API_Server  ──▶  ORM  ──▶  MySQL
 (Handy)                 (Server)        (EF Core)   (DB)
```

---

## 2️⃣ Die Vorzeige-Demo: Was beim LOGIN passiert ⭐

> Wenn du diesen einen Ablauf erklären kannst, hast du gezeigt, dass du **alle Schichten** verstehst. Das ist die wichtigste Frage.

1. **Du tippst Email + Passwort** in die `LoginPage`. Die Felder sind per **DataBinding** mit dem `LoginViewModel` verbunden (Properties `Email`, `Password`).
2. **Das ViewModel ruft einen Service**, nicht direkt den Server: `_authService.LoginAsync(...)`. → *UI-Code macht nie selbst HTTP-Anfragen.*
3. **`AuthService` schickt** `POST /api/auth/login` mit den Daten als JSON.
4. **Der `AuthController` empfängt sie:**
   - sucht den User per Email
   - prüft das Passwort mit **BCrypt** (die DB speichert nur einen *Hash*, nie das echte Passwort)
   - bei Erfolg: `TokenService.GenerateToken()` erzeugt einen **JWT** und schickt ihn zurück
5. **Die App speichert den Token** (`TokenStorageService`) und wechselt zur Haupt-App (`AppShell`).
6. **Ab jetzt trägt jede Anfrage den Token:** `AuthHttpMessageHandler` hängt automatisch `Authorization: Bearer <token>` an – außer bei Login/Register.

**Der ganze Kreislauf:** `View → ViewModel → Service → API → Datenbank → und wieder zurück.`

---

## 3️⃣ Die 4 Konzepte, nach denen der Lehrer fragt

### ① JWT – das „Festival-Bändchen" 🎟️
Beim Login bekommst du einen signierten Token (wie ein Festival-Bändchen). Er enthält deine User-ID, ist mit einem geheimen Schlüssel **signiert** (nicht fälschbar) und **läuft ab**. Der Server merkt sich dich *nicht* zwischen Anfragen („stateless") – du zeigst das Bändchen jedes Mal neu.
- **Erstellt in:** `TokenService.cs` · **Geprüft in:** `Program.cs` (JWT-Validierung)
- **Warum JWT?** „Damit der Server keine Sitzung pro User speichern muss – der Token selbst beweist, wer ich bin."

### ② MVVM – warum es View + ViewModel Paare gibt 🧩
Die `.xaml`-View ist **nur** Layout. Das ViewModel hält Daten und Logik. Verbunden über **DataBinding**: ändere ich eine Property im ViewModel, aktualisiert sich der Bildschirm automatisch. `[ObservableProperty]` und `[RelayCommand]` (CommunityToolkit) erzeugen diese Verdrahtung automatisch.
- **Warum?** „Trennt UI von Logik – ich kann das Design ändern, ohne die Logik anzufassen, und es ist testbar."

### ③ Dependency Injection (die Listen in `MauiProgram.cs` / `Program.cs`) 🔌
Keine Klasse schreibt `new AuthService()`. Alles wird **einmal registriert** und vom Framework automatisch in die Konstruktoren **hineingereicht** (injiziert). Deshalb *bekommt* der `LoginViewModel`-Konstruktor einfach ein `IAuthService`.
- **Warum?** „Damit die Teile nicht fest verdrahtet sind – leicht austauschbar und testbar."

### ④ EF Core + Repositories – die Datenschicht 🗄️
`AppDbContext.cs` mappt C#-Klassen auf Datenbank-Tabellen. `OnModelCreating` legt Schlüssel, Beziehungen und **Cascade-Deletes** fest (lösche eine Reise → ihre Gruppen & Ausgaben verschwinden automatisch). Die **Repositories** (z.B. `TripRepository`) kapseln die Abfragen, damit Controller sauber bleiben.
- **Sicherheits-Detail zum Angeben:** `GetByIdForUserAsync` liefert eine Reise **nur, wenn du Mitglied bist** – fremde Reisen kann man nicht über die ID aufrufen.

---

## 4️⃣ Wahrscheinliche Fragen → Antworten in deinen Worten

**„Erklär mal, was beim Login passiert."**
→ Abschnitt 2 oben durchgehen. **Die wichtigste Frage.**

**„Woher weiß die App bei späteren Anfragen, wer eingeloggt ist?"**
→ „Über den JWT. Der wird automatisch von einem Message-Handler angehängt, und die API liest meine User-ID mit `User.GetUserId()` aus dem Token heraus."

**„Wo werden Passwörter gespeichert?"**
→ „Nie im Klartext – nur als BCrypt-Hash. Beim Login wird das eingegebene Passwort neu gehasht und mit dem gespeicherten Hash verglichen."

**„Warum überall HTTPS?"**
→ „Wenn die App über HTTP anfragt und auf HTTPS umgeleitet wird, geht der Authorization-Header verloren → stille 401-Fehler. Darum starten wir direkt mit HTTPS." *(Steht so in `ApiConfig.cs`.)*

**„Wie sieht die Datenbank-Struktur aus?"**
→ „Users, Trips, Groups, Expenses – plus Verknüpfungstabellen wie `TripMember`. Eine Reise hat viele Gruppen, eine Gruppe hat viele Ausgaben."

**„Wo wird berechnet, wer wem wie viel schuldet?"**
→ „Serverseitig im `GroupsController`, Methode `BuildBalances`. Pro User: *Summe bezahlt − eigener Anteil = Netto-Saldo*. Positiv = bekommt Geld, negativ = schuldet. Daraus werden die Transfers gebildet. Die App zeigt nur das fertige Ergebnis an."

**„Was ist ein Repository / wofür der Zwischenschritt?"**
→ „Eine Klasse, die alle Datenbank-Abfragen für einen Bereich (z.B. Trips) bündelt. So bleibt der Controller schlank und die DB-Logik an einer Stelle."

**„Was passiert, wenn der Token abläuft?"**
→ „Der Server antwortet mit 401. Der Client merkt das, löscht den Token und schickt mich zurück zum Login." *(Szenario „⏰ Session abgelaufen" im Diagramm.)*

---

## 5️⃣ Wichtige Dateien auf einen Blick

| Datei | Wofür |
|---|---|
| `Brokey_APP/ViewModels/LoginViewModel.cs` | Login-Logik (Vorzeige-Datei) |
| `Brokey_APP/Services/AuthService.cs` | HTTP-Anfragen an /api/auth |
| `Brokey_APP/Services/AuthHttpMessageHandler.cs` | hängt JWT automatisch an |
| `Brokey_APP/Services/TokenStorageService.cs` | Token speichern (3-stufig) |
| `Brokey_APP/MauiProgram.cs` | DI-Registrierung (Client) |
| `API_Server/Program.cs` | DI, JWT-Config, Middleware (Server) |
| `API_Server/Controllers/AuthController.cs` | Login/Register/Me |
| `API_Server/Controllers/GroupsController.cs` | Gruppen, Ausgaben, **Abrechnung** |
| `API_Server/Services/TokenService.cs` | erzeugt den JWT |
| `ORM/AppDbContext.cs` | DB-Schema & Beziehungen |
| `ORM/Repositories/TripRepository.cs` | DB-Abfragen für Reisen |
| `Models/*.cs` | gemeinsame Entitäten (User, Trip, …) |

---

## 6️⃣ Datenbank-Beziehungen (kurz)

```
User ──erstellt──▶ Trip ──hat──▶ Group ──hat──▶ Expense ──aufgeteilt in──▶ ExpenseSplit
                    │                                │
              TripMember                        bezahlt von: User
           (Wer ist dabei?)                     Kategorie: ExpenseCategory
```
- `TripMember` / `GroupMember` = **Verknüpfungstabellen** (n:m), mit Unique-Constraint gegen Doppel-Mitgliedschaften.
- **Cascade-Delete:** Reise löschen → Gruppen + Ausgaben gehen automatisch mit.
- **Geseedet:** 8 Ausgaben-Kategorien (Food, Transport, …) + 25 Land→Währung-Einträge.

---

## 7️⃣ Befehle, falls du etwas live zeigen musst

```bash
# API starten (WICHTIG: HTTPS-Profil!)
dotnet run --project API_Server/API_Server.csproj --launch-profile https
# → https://localhost:7221 , Swagger: http://localhost:5224/swagger

# MAUI-App bauen
dotnet build Brokey_APP/Brokey_APP.csproj -f net10.0-maccatalyst
```

---

## ✅ Spickzettel-Kurzfassung (wenn du nur 30 Sek. hast)
1. **4 Projekte:** App (UI) → API (Server) → ORM (DB-Zugriff) → Models (geteilt).
2. **Die App redet nie direkt mit der DB**, immer über die API per HTTPS.
3. **Login:** Passwort wird per BCrypt geprüft → Server gibt **JWT** zurück → App hängt ihn an jede Anfrage.
4. **MVVM:** View = Layout, ViewModel = Logik, verbunden per DataBinding.
5. **DI:** Alles wird in `Program.cs` / `MauiProgram.cs` registriert und automatisch eingesetzt.
6. **Abrechnung** wird serverseitig in `GroupsController.BuildBalances` berechnet.

**Du schaffst das! 💪**
