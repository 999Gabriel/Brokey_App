# Fortschritt & nächste Schritte

## Zuletzt abgeschlossen

- TripsPage redesigned: kompakter Header mit `+`-Button, aufgeräumte Trip-Karten
- AddMember-Flow vereinfacht: kein Role-Picker mehr, immer als „Member" hinzufügen
- Neue TripSummaryPage: Hero-Betrag, Tage/Ausgaben/Members-Kacheln, per-Gruppe Aufschlüsselung, Mitgliederliste
- Navigation: „Summary"-Button auf TripDetailPage → trip-summary Route
- App-Icon auf `brokey_appicon.png` umgestellt

---

## Nächste Session: Manueller Sync-Button

### Problem
Wenn zwei Geräte (z. B. Mac + iPhone) mit verschiedenen Accounts eingeloggt sind und eine neue Expense eingetragen wird, sieht das andere Gerät die Änderung nicht — bis zum nächsten Seitenaufruf.

### Ziel
Ein Refresh-Button (oder Pull-to-Refresh), der alle Daten vom Server neu lädt und sofort anzeigt.

### Was zu tun ist

#### Option A — Pull-to-Refresh (empfohlen, native iOS/Android-Geste)
- Jede Seite mit Live-Daten in einen `RefreshView` wrappen
- `RefreshView.Command` an den jeweiligen Load-Command des ViewModels binden
- `RefreshView.IsRefreshing` an `IsBusy` binden
- Betrifft: `TripsPage`, `TripDetailPage`, `GroupDetailPage`, `TripSummaryPage`

```xml
<RefreshView Command="{Binding LoadTripsCommand}"
             IsRefreshing="{Binding IsBusy}">
    <CollectionView ... />
</RefreshView>
```

#### Option B — Expliziter Reload-Button in der Toolbar
- `ToolbarItem` mit Refresh-Icon (⟳) in den Seiten-Toolbar einbauen
- Command zeigt auf denselben Load-Command

```xml
<ContentPage.ToolbarItems>
    <ToolbarItem Text="Reload" Command="{Binding LoadTripsCommand}" />
</ContentPage.ToolbarItems>
```

#### Welche Seiten brauchen es?
| Seite | Load-Command | Priorität |
|---|---|---|
| `TripsPage` | `LoadTripsCommand` | Hoch |
| `TripDetailPage` | `LoadTripCommand` | Hoch |
| `GroupDetailPage` | `LoadGroupCommand` | Hoch |
| `TripSummaryPage` | `LoadCommand` | Mittel |
| `HomePage` (Recent Activity) | `LoadActivitiesCommand` | Mittel |

#### Empfehlung
Pull-to-Refresh (Option A) ist die sauberere mobile UX. Option B kann zusätzlich als Toolbar-Icon für die Mac-Variante rein, wo kein „Pull" existiert. Beide können parallel implementiert werden.

### Technische Hinweise
- `RefreshView` in MAUI ist in `Styles.xaml` bereits mit `RefreshColor` gestylt (`Primary`)
- `IsBusy` ist in `BaseViewModel` vorhanden — passt direkt als `IsRefreshing`-Binding
- Kein neuer API-Endpoint nötig — alle Load-Commands rufen bereits die richtigen Endpoints auf
