# Brokey App Knowledge Base

This file is a shared reference for what we know about the app and what we learned while building it.

## 1. Current Architecture
- **Client:** `.NET MAUI` in `Brokey_APP/` (MVVM: `Views/`, `ViewModels/`, `Services/`).
- **API:** `ASP.NET Core` in `API_Server/` (controllers + JWT auth).
- **Data layer:** EF Core in `ORM/` with repositories and migrations.
- **Shared models:** `Models/` used across API/client domains.

## 2. Auth Design (Current, Working)
- JWT is issued by `POST /api/auth/login` and `POST /api/auth/register`.
- Protected endpoints (`/api/trips`, `/api/groups/...`, `/api/auth/me`) require Bearer token.
- API auth setup:
  - `API_Server/Program.cs` (`AddAuthentication`, `AddJwtBearer`, `UseAuthentication`, `UseAuthorization`)
  - `API_Server/Services/TokenService.cs`
  - `API_Server/Controllers/AuthController.cs`
- Client token flow:
  - `AuthService` logs in/registers and saves token.
  - `AuthHttpMessageHandler` injects `Authorization: Bearer <token>` on non-auth requests.
  - `TokenStorageService` stores token with memory cache + `Preferences` primary + `SecureStorage` sync.

## 3. Major Lessons Learned
- The biggest auth bug was **not JWT itself**; it was transport mismatch.
- If client calls API over `http` and API redirects to `https`, Bearer headers may be lost on redirect.
- This caused: login looked successful, but `Trips` requests returned `401` and user got kicked out.
- Fix: call API directly with HTTPS in `Brokey_APP/Services/ApiConfig.cs`.
- We also saw stale token behavior from storage fallback. We fixed by:
  - Clearing token before new login/register.
  - Favoring fresh in-memory/preferences token.
  - Clearing token on unauthorized responses.

## 4. Feature Status Snapshot
- Working: register, login, logout, load profile, create/load trips, create groups, add/remove/list group members.
- Navigation path working: dashboard -> trips -> trip detail -> group detail.

## 5. Local Run Checklist
1. Start API in Rider with **https** profile (`https://localhost:7221`).
2. Start MAUI app.
3. Log in/register fresh.
4. Open Trips and verify load/create.

## 6. Release-Readiness Notes
- Keep JWT (production-ready pattern for mobile APIs).
- Keep HTTPS-only API base URLs.
- Do not hardcode secrets for production (move JWT key/connection strings to secure config).
- Add automated integration tests for auth + protected trip/group endpoints.

---

## 7. App Icon Setup (inkl. macOS Fix)

### Standard-Konfiguration (iOS & Android)

**Schritt 1:** Icon-Quelldatei (PNG, mindestens 1024×1024 px) ablegen:
```
Brokey_APP/Resources/AppIcon/brokey_appicon.png
```

**Schritt 2:** Im `.csproj` referenzieren:
```xml
<ItemGroup>
    <MauiIcon Include="Resources\AppIcon\brokey_appicon.png"/>
</ItemGroup>
```

Das ist für iOS und Android ausreichend. MAUIs `Resizetizer`-Tool generiert beim Build automatisch alle benötigten Größen und verpackt sie als `Assets.xcassets` im `obj/`-Verzeichnis.

---

### Das macOS-Problem (Mac Catalyst)

**Ursache:** MAUIs Resizetizer generiert in der `Contents.json` nur **iOS-Idiome** (`iphone`, `ipad`). Für macOS braucht actool aber den **`mac`-Idiom**. Ohne ihn zeigt macOS den generischen Platzhalter-Icon.

Außerdem muss `Platforms/MacCatalyst/Info.plist` den Key `XSAppIconAssets` auf das korrekte `.appiconset` zeigen – der Name leitet sich vom Dateinamen der Quell-PNG ab (`brokey_appicon.png` → `brokey_appicon.appiconset`).

---

### Fix: Manuelles `Assets.xcassets` für macOS

**Dateistruktur:**
```
Platforms/MacCatalyst/
└── Assets.xcassets/
    ├── Contents.json                    ← Pflicht: leere xcassets-Root
    └── AppIcon.appiconset/
        ├── Contents.json                ← mac-Idiom-Einträge
        ├── icon_16x16.png
        ├── icon_32x32.png
        ├── icon_64x64.png
        ├── icon_128x128.png
        ├── icon_256x256.png
        ├── icon_512x512.png
        └── icon_1024x1024.png
```

MAUI bindet Dateien unter `Platforms/MacCatalyst/` automatisch als `ImageAsset`-Items ein – kein extra Eintrag im `.csproj` nötig.

**Icon-Größen generieren (macOS `sips`):**
```bash
SRC="Resources/AppIcon/brokey_appicon.png"
DEST="Platforms/MacCatalyst/Assets.xcassets/AppIcon.appiconset"

for size in 16 32 64 128 256 512 1024; do
  sips -z $size $size "$SRC" --out "$DEST/icon_${size}x${size}.png"
done
```

**`Assets.xcassets/Contents.json` (Root):**
```json
{
  "info": { "version": 1, "author": "xcode" }
}
```

**`AppIcon.appiconset/Contents.json`:**
```json
{
  "images": [
    { "idiom": "mac", "size": "16x16",   "scale": "1x", "filename": "icon_16x16.png"   },
    { "idiom": "mac", "size": "16x16",   "scale": "2x", "filename": "icon_32x32.png"   },
    { "idiom": "mac", "size": "32x32",   "scale": "1x", "filename": "icon_32x32.png"   },
    { "idiom": "mac", "size": "32x32",   "scale": "2x", "filename": "icon_64x64.png"   },
    { "idiom": "mac", "size": "128x128", "scale": "1x", "filename": "icon_128x128.png" },
    { "idiom": "mac", "size": "128x128", "scale": "2x", "filename": "icon_256x256.png" },
    { "idiom": "mac", "size": "256x256", "scale": "1x", "filename": "icon_256x256.png" },
    { "idiom": "mac", "size": "256x256", "scale": "2x", "filename": "icon_512x512.png" },
    { "idiom": "mac", "size": "512x512", "scale": "1x", "filename": "icon_512x512.png" },
    { "idiom": "mac", "size": "512x512", "scale": "2x", "filename": "icon_1024x1024.png" }
  ],
  "info": { "version": 1, "author": "xcode" }
}
```

**`Platforms/MacCatalyst/Info.plist` – `XSAppIconAssets` anpassen:**
```xml
<key>XSAppIconAssets</key>
<string>Assets.xcassets/AppIcon.appiconset</string>
```

---

### Wie der Build-Prozess die Teile zusammensetzt

```
MauiIcon (csproj)
  └─► Resizetizer
        └─► obj/.../Assets.xcassets/brokey_appicon.appiconset/   ← iOS-only, wird für mac ignoriert

Platforms/MacCatalyst/Assets.xcassets/                           ← auto-included von MAUI
  └─► actool (Apple Asset Compiler)
        └─► AppIcon.icns → Brokey_APP.app/Contents/Resources/AppIcon.icns

Info.plist (CFBundleIconFile = "AppIcon")
  └─► macOS lädt AppIcon.icns aus dem Bundle
```

---

### Verification nach dem Build

```bash
# ICNS vorhanden?
ls Brokey_APP/bin/Debug/net10.0-maccatalyst/.../Brokey_APP.app/Contents/Resources/AppIcon.icns

# Info.plist korrekt?
plutil -p ...Brokey_APP.app/Contents/Info.plist | grep icon

# Icon-Inhalt visuell prüfen
sips -s format png ...AppIcon.icns --out /tmp/preview.png && open /tmp/preview.png
```

---

### Checkliste

| Was                         | Wo                                                          | Plattform |
|-----------------------------|-------------------------------------------------------------|-----------|
| Quelldatei (≥1024×1024 PNG) | `Resources/AppIcon/brokey_appicon.png`                      | Alle      |
| MauiIcon-Eintrag            | `.csproj`                                                   | Alle      |
| XSAppIconAssets (iOS)       | `Platforms/iOS/Info.plist` → `brokey_appicon.appiconset`    | iOS       |
| XSAppIconAssets (macOS)     | `Platforms/MacCatalyst/Info.plist` → `AppIcon.appiconset`   | macOS     |
| mac-Idiom xcassets          | `Platforms/MacCatalyst/Assets.xcassets/AppIcon.appiconset/` | macOS     |
| Icon-PNGs (alle mac-Größen) | Gleicher Ordner, via `sips` generiert                       | macOS     |

---

## 8. Stale Build Artefakte — Das actool-Problem

### Was passiert

Wenn ein App-Icon umbenannt wird, speichert MSBuild einen Zwischenzustand in `obj/`. Der Build-Task `_CompileAppManifest` erkennt keine Änderung und überspringt sich selbst ("up-to-date"). Dadurch bleibt eine alte `AppManifest.plist` im `obj/`-Verzeichnis, die noch den alten Icon-Namen enthält. `actool` liest diese Datei und sucht das Icon nach dem alten Namen — es findet es nicht → Build-Fehler.

**Fehlermeldung:**
```
actool: None of the input catalogs contained a matching icon set named "brokey_travel_icon"
```

### Die drei Quellen des Problems

1. **`Info.plist` hat `XSAppIconAssets` hardcoded** → dieser Wert wird von `_CompileAppManifest` in die generierte `AppManifest.plist` gemergt. Selbst nach einem `dotnet clean` wird er beim nächsten Build wieder aus `Info.plist` hineinkopiert. Das ist die eigentliche Wurzel.

2. **`.csproj` referenziert eine nicht existierende Datei** → wenn `<MauiIcon Include="...">` auf eine Datei zeigt, die nicht existiert (z. B. Tippfehler im Dateinamen), bricht Resizetizer mit "Unable to find background file" ab.

3. **Stale `obj/`-Verzeichnis** → nach Icon-Umbenennung kann die alte `AppManifest.plist` bestehen bleiben, selbst wenn `Info.plist` und `.csproj` bereits korrekt sind.

### Fix-Reihenfolge

```bash
# 1. Info.plist korrigieren (XSAppIconAssets auf neuen Namen zeigen)
# 2. .csproj korrigieren (exakter Dateiname inkl. Unterstriche/Suffixe prüfen)
# 3. Stale Artefakte löschen
rm -rf Brokey_APP/obj/Debug/net10.0-ios/iossimulator-arm64/
rm -rf Brokey_APP/obj/Debug/net10.0-maccatalyst/
```

### Lesson: `dotnet clean` reicht nicht immer

`dotnet clean` löscht `bin/` und Teile von `obj/`, aber nicht alle Unterverzeichnisse. Insbesondere `obj/Debug/net10.0-ios/iossimulator-arm64/` kann nach einem Clean noch stehen. Im Zweifel: das `obj/Debug/`-Verzeichnis manuell löschen.

### Name-Konvention: `.appiconset` leitet sich vom PNG-Dateinamen ab

| PNG-Dateiname              | generiertes `.appiconset`           |
|----------------------------|-------------------------------------|
| `brokey_appicon.png`       | `brokey_appicon.appiconset`         |
| `brokey_icon_1024.png`     | `brokey_icon_1024.appiconset`       |
| `brokey_travel_icon.png`   | `brokey_travel_icon.appiconset`     |

→ `XSAppIconAssets` in `Info.plist` muss immer auf denselben Namen zeigen wie das PNG in `<MauiIcon Include="...">`.

---

## 9. Neue Seite in MAUI anlegen — vollständiger Ablauf

Wenn eine komplett neue Seite (View + ViewModel + Navigation) gebraucht wird, müssen immer **vier Stellen** angepasst werden. Am Beispiel der `TripSummaryPage`:

### Schritt 1: ViewModel erstellen

`ViewModels/TripSummaryViewModel.cs`

- Erbt von `BaseViewModel` (liefert `IsBusy`, `Title`)
- Implementiert `IQueryAttributable` wenn die Seite Navigations-Parameter empfängt
- `[ObservableProperty]` für alle Felder die die View bindet
- `[RelayCommand]` für alle Commands
- `ApplyQueryAttributes` liest Query-Parameter aus der Navigation URL:

```csharp
public void ApplyQueryAttributes(IDictionary<string, object> query)
{
    if (query.TryGetValue("tripId", out var raw) &&
        int.TryParse(raw?.ToString(), out var id))
    {
        TripId = id;
        _ = LoadAsync();  // sofort laden wenn Parameter ankommen
    }
}
```

### Schritt 2: View erstellen

`Views/TripSummaryPage.xaml` + `Views/TripSummaryPage.xaml.cs`

- Die `.xaml` Datei referenziert `x:DataType="vm:TripSummaryViewModel"` für Compile-Time-Binding-Checks
- Die `.xaml.cs` Datei empfängt das ViewModel per DI-Konstruktor und setzt `BindingContext`:

```csharp
public partial class TripSummaryPage : AnimatedContentPage
{
    public TripSummaryPage(TripSummaryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
```

### Schritt 3: DI registrieren

`MauiProgram.cs` — beide müssen als `Transient` registriert werden (neue Instanz pro Navigation):

```csharp
builder.Services.AddTransient<TripSummaryViewModel>();
builder.Services.AddTransient<TripSummaryPage>();
```

### Schritt 4: Route registrieren

`AppShell.xaml.cs`:

```csharp
Routing.RegisterRoute("trip-summary", typeof(TripSummaryPage));
```

### Navigation aufrufen (mit Query-Parameter)

```csharp
await Shell.Current.GoToAsync($"trip-summary?tripId={TripId}");
```

→ MAUI übergibt automatisch den Query-String als Dictionary an `ApplyQueryAttributes`.

### Zurück navigieren

```csharp
await Shell.Current.GoToAsync("..");
```

---

## 10. MAUI MVVM-Muster — Zusammenfassung der verwendeten Patterns

### ObservableProperty + RelayCommand (CommunityToolkit.Mvvm)

```csharp
[ObservableProperty]
private string _name = string.Empty;

[RelayCommand]
private async Task LoadAsync() { ... }
```

Der Toolkit-Generator erzeugt daraus:
- `Name` Property mit `OnPropertyChanged`
- `LoadAsyncCommand` (ein `IAsyncRelayCommand`)
- `LoadCommand` (Kurzform ohne Async-Suffix) als Alias

**Wichtig:** `[ObservableProperty]` erzeugt die Public-Property automatisch. Den `_privateField` nie direkt in der View binden — immer die generierte `PascalCase` Property verwenden.

### Computed Properties manuell benachrichtigen

Wenn eine berechnete Property (kein `[ObservableProperty]`) von einer anderen abhängt:

```csharp
public bool HasGroups => Groups.Count > 0;

// Nach dem Befüllen von Groups:
OnPropertyChanged(nameof(HasGroups));
```

### IsBusy für Ladezustand und Button-Deaktivierung

`BaseViewModel` hat `IsBusy`. In der View:

```xml
<Button IsEnabled="{Binding IsBusy, Converter={StaticResource InvertedBoolConverter}}" />
```

Damit ist der Button während des API-Calls deaktiviert.

---

## 11. XAML UI-Patterns in diesem Projekt

### SurfaceCard (Standard-Karte)

```xml
<Border StyleClass="SurfaceCard">
    <!-- Inhalt -->
</Border>
```

Definiert in `Styles.xaml`: weißer Hintergrund, `BorderSubtle` Rahmen, `RoundRectangle 18`, Schatten, `Margin="0,0,0,12"`.

### TagPill (Badge/Label)

```xml
<Border StyleClass="TagPill">
    <Label Text="OWNER" Style="{StaticResource MonoTag}" />
</Border>
```

Lila Hintergrund (`Secondary`), abgerundete Pill-Form, für Statuskennzeichnungen wie Währung, Rolle, Anzahl.

### SurfaceCardMuted (Eingabefelder-Container)

```xml
<Border StyleClass="SurfaceCardMuted">
    <Entry Placeholder="..." Text="{Binding ...}" />
</Border>
```

Hellgrauer Hintergrund, subtile Umrandung — gibt Eingabefeldern eine visuelle Box ohne eigenes Styling auf dem Entry selbst.

### Grid mit fixen Spalten für Header-Layouts

```xml
<Grid ColumnDefinitions="*,Auto">
    <Label Text="Titel" Style="{StaticResource PageTitle}" />
    <Button Grid.Column="1" Text="+" ... />
</Grid>
```

Muster: linke Spalte nimmt verfügbaren Platz (`*`), rechte Spalte passt sich der Button-Größe an (`Auto`).

### BindableLayout vs. CollectionView

| `BindableLayout` | `CollectionView` |
|---|---|
| Für kleine Listen (&lt;20 Items) | Für lange, scrollbare Listen |
| Items werden alle gleichzeitig gerendert | Virtualisiert (nur sichtbare Items) |
| Kein Scrolling-Support | Eingebaut |
| Direkt in `VerticalStackLayout` | Eigenes Element |
| `BindableLayout.ItemsSource` | `ItemsSource` |

In diesem Projekt: Mitgliederlisten → `BindableLayout`, Trip-Listen → `CollectionView`.

---

## 12. Backend-Regeln die die UI-Entscheidungen beeinflussen

### Gruppenmitglieder hinzufügen (`POST /api/groups/{id}/members`)

- Der Backend-Endpunkt akzeptiert `Role: "Admin"` oder `"Member"` (validiert via `NormalizeGroupRole`)
- Wenn ein User zu einer Gruppe hinzugefügt wird, wird er **automatisch auch als Trip-Member** eingetragen (`_tripMemberRepository.AddParticipantAsync`)
- Man braucht keinen separaten Trip-Member-Invite: der Gruppen-Add deckt beides ab

**UI-Entscheidung daraus:** Den Role-Picker aus `AddMemberPage` entfernt. Normale User haben keinen Grund, jemanden als Admin einzuladen. Das ViewModel sendet jetzt immer `"Member"`.

### Expense-Erstellung: Wer darf was

- `SplitMode = Equal` → jeder Member darf Expenses anlegen
- `SplitMode = Percentage` oder `Amount` → nur Admins der Gruppe dürfen das
- `PaidByUserId` ist optional: ohne Angabe wird automatisch der erste Admin der Gruppe als Zahler gesetzt

### Settlement-Daten sind group-scoped

- Es gibt keinen trip-weiten Settlement-Endpoint
- `GET /api/groups/{id}/settlement` gibt Balances + Transfers nur für eine Gruppe zurück
- Für trip-weite Übersicht müsste man N Calls machen (einen pro Gruppe) und client-seitig aggregieren
- **Aktuelle TripSummaryPage** nutzt nur `TripDetailResponse` (Groups mit `TotalExpenseAmount`) — kein Settlement, dafür kein N+1-Problem

---

## 13. TripsPage Redesign — Was und Warum

### Ursprünglicher Zustand (chaotisch)

Die Seite hatte einen 170px hohen Banner-Block mit:
- Zwei überlagerten Ellipsen-Blobs (Hintergrund-Dekoration)
- `"Brokey"` Ghosttext (groß, halb-transparent)
- Mascot-Bild mit Translation-Offsets
- Speech-Bubble-Box mit rotiertem Bubble-Tail (45°-Rotation)

Das fühlte sich chaotisch an, weil viele Elemente übereinander lagen, jedes mit eigenen Translations und ZIndex-Werten.

### Die Entscheidung

Der Mascot-Banner bleibt auf der **HomePage** (passt dort als Begrüßungs-Element). Auf der **TripsPage** wurde er entfernt — diese Seite hat eine klare Funktion (Trips auflisten), braucht kein dekoratives Element.

### Neues Layout

```
[Your trips                          +]   ← Grid: Titel links, + Button rechts
[All shared journeys in one place.    ]

[Trip Name                      EUR  ]   ← Karten
[12 Jan 2025 – 22 Jan 2025           ]
[3 groups  4 members             ›   ]
```

Badges für Groups/Members statt plain Text: `SurfaceSubtle`-Border mit `RoundRectangle 8`.

### "Create trip"-Button

War unten als volle Breite. Jetzt ist es der `+`-Button oben rechts im Header. Das ist kompakter und verhindert, dass die Seite mit einem Button unten „voll" wirkt.

---

## 14. Wie `TripDetailResponse` aufgebaut ist

`TripDetailResponse` ist das reichste Datenobjekt in der App. Es wird von `GET /api/trips/{id}` geliefert und enthält:

```csharp
public class TripDetailResponse
{
    public int Id, DurationDays, GroupCount, MemberCount, ExpenseCount;
    public string Name, Description, BaseCurrency, CreatedByUsername;
    public DateTime StartDate, EndDate, CreatedAt;
    public decimal TotalExpenseAmount;          // Summe aller Expenses im Trip
    public List<TripMemberResponse> Members;    // alle Trip-Mitglieder (ordered: Owner first)
    public List<GroupResponse> Groups;          // alle Gruppen (ordered by Name)
}
```

Jede `GroupResponse` enthält:
```csharp
public int Id, TripId, CreatedById, MemberCount, ExpenseCount;
public string Name;
public decimal TotalExpenseAmount;   // Summe aller Expenses in dieser Gruppe
public DateTime CreatedAt;
```

→ `TripDetailResponse` reicht für eine vollständige Trip Summary (Gesamtbetrag + pro-Gruppe-Aufschlüsselung) **ohne weitere API-Calls**.

---

## 15. App-Icon wechseln — Kurzanleitung

1. Neue PNG-Datei (≥1024×1024) nach `Brokey_APP/Resources/AppIcon/` legen
2. `.csproj` → `<MauiIcon Include="Resources\AppIcon\NEUER_DATEINAME.png"/>`
3. `Platforms/iOS/Info.plist` → `XSAppIconAssets` auf `Assets.xcassets/NEUER_DATEINAME.appiconset`
4. Stale Artefakte löschen:
   ```bash
   rm -rf Brokey_APP/obj/Debug/net10.0-ios/
   rm -rf Brokey_APP/obj/Debug/net10.0-maccatalyst/
   ```
5. Build starten — Resizetizer generiert das `.appiconset` automatisch aus der neuen PNG
