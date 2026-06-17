using Microsoft.EntityFrameworkCore;
using Models;

namespace ORM;

// Zentrale Datenbank-Klasse von EF Core: Sie ist das "Tor" zur MySQL-Datenbank "Brokey".
// Jede DbSet<T>-Property unten entspricht GENAU einer Tabelle. Über diese Sets werden
// mit LINQ Datensätze abgefragt, hinzugefügt, geändert und gelöscht.
// Die Repositories greifen über _context auf genau diese DbSets zu.
public class AppDbContext : DbContext
{
    // Jede dieser Properties ist eine Tabelle in der Datenbank.
    // Set<T>() liefert das DbSet, mit dem man die jeweilige Tabelle abfragt/bearbeitet.
    public DbSet<User> Users => Set<User>();                         // Tabelle "Users": registrierte Benutzer (Login-Daten, Profil).
    public DbSet<Group> Groups => Set<Group>();                      // Tabelle "Groups": Untergruppen innerhalb eines Trips.
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();    // Tabelle "GroupMembers": Zuordnung Benutzer ↔ Gruppe (n:m-Verknüpfung).
    public DbSet<Trip> Trips => Set<Trip>();                         // Tabelle "Trips": Reisen, die Ausgaben/Gruppen bündeln.
    public DbSet<TripMember> TripMembers => Set<TripMember>();       // Tabelle "TripMembers": Zuordnung Benutzer ↔ Trip (n:m-Verknüpfung).
    public DbSet<Expense> Expenses => Set<Expense>();                // Tabelle "Expenses": einzelne Ausgaben (wer hat wie viel bezahlt).
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>(); // Tabelle "ExpenseCategories": Kategorien (Food, Transport ...), vorgeseedet.
    public DbSet<ExpenseSplit> ExpenseSplits => Set<ExpenseSplit>(); // Tabelle "ExpenseSplits": Aufteilung einer Ausgabe auf einzelne Benutzer.
    public DbSet<Receipt> Receipts => Set<Receipt>();               // Tabelle "Receipts": Belegfotos zu einer Ausgabe.
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>(); // Tabelle "ExchangeRates": Wechselkurse zwischen zwei Währungen.
    public DbSet<CountryCurrency> CountryCurrencies => Set<CountryCurrency>(); // Tabelle "CountryCurrencies": Land → Währungscode, vorgeseedet.

    // Konstruktor: Die Optionen (z.B. Connection-String) werden von außen (Program.cs per DI) injiziert.
    // So weiß EF Core, mit welcher Datenbank es sich verbinden soll.
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // OnModelCreating wird von EF Core EINMAL beim Aufbau des Datenmodells aufgerufen.
    // Hier wird mit der "Fluent API" das komplette Datenbank-Schema beschrieben:
    //  - Primärschlüssel (HasKey)
    //  - Pflichtfelder/Längen (IsRequired/HasMaxLength) und Genauigkeit (HasPrecision)
    //  - Indizes/Unique-Constraints (HasIndex/IsUnique)
    //  - Beziehungen zwischen Tabellen (HasOne/WithMany/HasForeignKey)
    //  - Lösch-Regeln (OnDelete: Cascade = mitlöschen, Restrict = blockieren, SetNull = Verweis leeren)
    // Am Ende werden mit HasData feste Startdaten (Kategorien + Währungen) geseedet.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Ruft die Basis-Implementierung von DbContext auf (EF-Core-Konvention).

        // ── User (Benutzerkonto) ──
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);                                       // Primärschlüssel = Id (eindeutige Benutzer-Nummer, auto-increment).
            entity.Property(u => u.Username).IsRequired().HasMaxLength(50); // Username: Pflichtfeld (NOT NULL), max. 50 Zeichen.
            entity.Property(u => u.Email).IsRequired().HasMaxLength(100);   // Email: Pflichtfeld, max. 100 Zeichen.
            entity.HasIndex(u => u.Email).IsUnique();                       // Unique-Index auf Email: keine zwei Benutzer mit derselben E-Mail (verhindert Doppel-Registrierung).
            entity.Property(u => u.PasswordHash).IsRequired();             // Passwort wird nur als Hash gespeichert, nie im Klartext; Pflichtfeld.
            entity.Property(u => u.BaseCurrency).IsRequired().HasMaxLength(3); // Standardwährung des Users als 3-stelliger ISO-Code (z.B. "EUR").
            entity.Property(u => u.ProfileImagePath).HasMaxLength(500);    // Optionaler Pfad zum Profilbild (kein IsRequired → darf NULL sein).
        });

        // ── Group (Gruppe innerhalb eines Trips) ──
        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(g => g.Id);                                        // Primärschlüssel = Id.
            entity.Property(g => g.Name).IsRequired().HasMaxLength(100);     // Gruppenname: Pflichtfeld, max. 100 Zeichen.
            entity.HasIndex(g => new { g.TripId, g.Name });                  // Kombinierter Index (TripId, Name): beschleunigt das Suchen von Gruppen innerhalb eines Trips (NICHT unique).

            // Beziehung Group → Trip: Eine Gruppe gehört zu genau EINEM Trip; ein Trip hat VIELE Gruppen.
            entity.HasOne(g => g.Trip)                                       // Jede Gruppe hat einen Trip ...
                .WithMany(t => t.Groups)                                     // ... und ein Trip hat mehrere Gruppen (Navigation Trip.Groups).
                .HasForeignKey(g => g.TripId)                               // Fremdschlüssel-Spalte ist TripId.
                .OnDelete(DeleteBehavior.Cascade);                         // Cascade: Wird der Trip gelöscht, werden automatisch auch seine Gruppen gelöscht.

            // Beziehung Group → User (Ersteller): Wer hat die Gruppe angelegt?
            entity.HasOne(g => g.CreatedBy)                                  // Jede Gruppe hat einen Ersteller (User) ...
                .WithMany(u => u.CreatedGroups)                             // ... ein User kann viele Gruppen erstellt haben.
                .HasForeignKey(g => g.CreatedById)                         // Fremdschlüssel ist CreatedById.
                .OnDelete(DeleteBehavior.Restrict);                       // Restrict: Ein User kann NICHT gelöscht werden, solange er noch Gruppen erstellt hat (schützt vor Datenverlust).
        });

        // ── GroupMember (Verknüpfungstabelle Benutzer ↔ Gruppe) ──
        modelBuilder.Entity<GroupMember>(entity =>
        {
            entity.HasKey(gm => gm.Id);                                       // Primärschlüssel = Id.
            entity.HasIndex(gm => new { gm.GroupId, gm.UserId }).IsUnique();  // Unique-Constraint (GroupId, UserId): verhindert, dass derselbe User ZWEIMAL in derselben Gruppe ist.
            entity.Property(gm => gm.Role).IsRequired().HasMaxLength(20);     // Rolle (z.B. "Admin"/"Member"): Pflichtfeld, max. 20 Zeichen.

            // Beziehung GroupMember → Group.
            entity.HasOne(gm => gm.Group)                                    // Jeder Eintrag gehört zu einer Gruppe ...
                .WithMany(g => g.Members)                                    // ... eine Gruppe hat viele Mitglieder (Group.Members).
                .HasForeignKey(gm => gm.GroupId)
                .OnDelete(DeleteBehavior.Cascade);                          // Cascade: Wird die Gruppe gelöscht, verschwinden auch ihre Mitgliedschafts-Einträge.

            // Beziehung GroupMember → User.
            entity.HasOne(gm => gm.User)                                     // Jeder Eintrag gehört zu einem User ...
                .WithMany(u => u.GroupMemberships)                          // ... ein User kann in vielen Gruppen sein (User.GroupMemberships).
                .HasForeignKey(gm => gm.UserId)
                .OnDelete(DeleteBehavior.Cascade);                          // Cascade: Wird der User gelöscht, verschwinden auch seine Gruppen-Mitgliedschaften.
        });

        // ── Trip (Reise) ──
        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(t => t.Id);                                          // Primärschlüssel = Id.
            entity.Property(t => t.Name).IsRequired().HasMaxLength(150);       // Reisename: Pflichtfeld, max. 150 Zeichen.
            entity.Property(t => t.Description).HasMaxLength(500);             // Beschreibung: optional (kein IsRequired), max. 500 Zeichen.
            entity.Property(t => t.BaseCurrency).IsRequired().HasMaxLength(3); // Basiswährung des Trips als 3-stelliger ISO-Code; Pflichtfeld.

            // Beziehung Trip → User (Ersteller).
            entity.HasOne(t => t.CreatedBy)                                    // Jeder Trip hat einen Ersteller ...
                .WithMany(u => u.CreatedTrips)                                // ... ein User kann mehrere Trips erstellt haben (User.CreatedTrips).
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);                          // Restrict: Ein User kann nicht gelöscht werden, solange er noch Trips erstellt hat.
        });

        // ── TripMember (Verknüpfungstabelle Benutzer ↔ Trip) ──
        modelBuilder.Entity<TripMember>(entity =>
        {
            entity.HasKey(tm => tm.Id);                                       // Primärschlüssel = Id.
            entity.HasIndex(tm => new { tm.TripId, tm.UserId }).IsUnique();   // Unique-Constraint (TripId, UserId): derselbe User kann nicht doppelt im selben Trip sein.
            entity.Property(tm => tm.Role).IsRequired().HasMaxLength(20);     // Rolle (z.B. "Owner"/"Member"): Pflichtfeld, max. 20 Zeichen.

            // Beziehung TripMember → Trip.
            entity.HasOne(tm => tm.Trip)                                     // Jeder Eintrag gehört zu einem Trip ...
                .WithMany(t => t.Members)                                    // ... ein Trip hat viele Mitglieder (Trip.Members).
                .HasForeignKey(tm => tm.TripId)
                .OnDelete(DeleteBehavior.Cascade);                          // Cascade: Wird der Trip gelöscht, verschwinden auch die TripMember-Einträge.

            // Beziehung TripMember → User.
            entity.HasOne(tm => tm.User)                                     // Jeder Eintrag gehört zu einem User ...
                .WithMany(u => u.TripMemberships)                           // ... ein User kann an vielen Trips teilnehmen (User.TripMemberships).
                .HasForeignKey(tm => tm.UserId)
                .OnDelete(DeleteBehavior.Cascade);                          // Cascade: Wird der User gelöscht, verschwinden auch seine Trip-Mitgliedschaften.
        });

        // ── ExpenseCategory (Ausgaben-Kategorie, z.B. Food, Transport) ──
        modelBuilder.Entity<ExpenseCategory>(entity =>
        {
            entity.HasKey(ec => ec.Id);                                      // Primärschlüssel = Id.
            entity.Property(ec => ec.Name).IsRequired().HasMaxLength(50);    // Kategoriename: Pflichtfeld, max. 50 Zeichen.
            entity.HasIndex(ec => ec.Name).IsUnique();                      // Unique-Index: jeder Kategoriename darf nur einmal existieren.
            entity.Property(ec => ec.Icon).HasMaxLength(50);               // Icon (Emoji wie "🍔"): optional, max. 50 Zeichen.
        });

        // ── Expense (einzelne Ausgabe) ──
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.Id);                                        // Primärschlüssel = Id.
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);    // Titel der Ausgabe: Pflichtfeld, max. 200 Zeichen.
            entity.Property(e => e.Description).HasMaxLength(500);          // Beschreibung: optional, max. 500 Zeichen.
            entity.Property(e => e.Amount).HasPrecision(18, 2);            // Betrag als decimal(18,2): 18 Stellen gesamt, 2 Nachkommastellen (Cent-genau, keine Rundungsfehler wie bei double).
            entity.Property(e => e.Latitude).HasPrecision(10, 7);          // Breitengrad (optionaler Standort) als decimal(10,7).
            entity.Property(e => e.Longitude).HasPrecision(10, 7);         // Längengrad als decimal(10,7).
            entity.HasIndex(e => new { e.GroupId, e.ExpenseDate });        // Index (GroupId, ExpenseDate): beschleunigt das Laden+Sortieren der Ausgaben einer Gruppe nach Datum.

            // Beziehung Expense → Trip: zu welcher Reise gehört die Ausgabe?
            entity.HasOne(e => e.Trip)
                .WithMany(t => t.Expenses)
                .HasForeignKey(e => e.TripId)
                .OnDelete(DeleteBehavior.Cascade);                         // Cascade: Trip gelöscht → alle seine Ausgaben werden mitgelöscht.

            // Beziehung Expense → Group: optionale Zuordnung zu einer Gruppe.
            entity.HasOne(e => e.Group)
                .WithMany(g => g.Expenses)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.SetNull);                        // SetNull: Gruppe gelöscht → GroupId der Ausgabe wird NULL, die Ausgabe selbst bleibt aber erhalten (geht nicht verloren).

            // Beziehung Expense → User (Zahler): wer hat ausgelegt?
            entity.HasOne(e => e.PaidBy)
                .WithMany(u => u.PaidExpenses)
                .HasForeignKey(e => e.PaidByUserId)
                .OnDelete(DeleteBehavior.Restrict);                       // Restrict: User mit bezahlten Ausgaben kann nicht gelöscht werden.

            // Beziehung Expense → ExpenseCategory: welcher Kategorie ist die Ausgabe zugeordnet?
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Expenses)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);                       // Restrict: Eine Kategorie kann nicht gelöscht werden, solange noch Ausgaben darauf verweisen.
        });

        // ── ExpenseSplit (Anteil eines Users an einer Ausgabe) ──
        modelBuilder.Entity<ExpenseSplit>(entity =>
        {
            entity.HasKey(es => es.Id);                                      // Primärschlüssel = Id.
            entity.Property(es => es.Amount).HasPrecision(18, 2);          // Anteilsbetrag als decimal(18,2) (Cent-genau).

            // Beziehung ExpenseSplit → Expense: zu welcher Ausgabe gehört der Anteil?
            entity.HasOne(es => es.Expense)
                .WithMany(e => e.Splits)
                .HasForeignKey(es => es.ExpenseId)
                .OnDelete(DeleteBehavior.Cascade);                         // Cascade: Ausgabe gelöscht → ihre Splits werden mitgelöscht.

            // Beziehung ExpenseSplit → User: welcher User schuldet diesen Anteil?
            entity.HasOne(es => es.User)
                .WithMany(u => u.ExpenseSplits)
                .HasForeignKey(es => es.UserId)
                .OnDelete(DeleteBehavior.Cascade);                         // Cascade: User gelöscht → seine Anteils-Einträge werden mitgelöscht.
        });

        // ── Receipt (Belegfoto zu einer Ausgabe) ──
        modelBuilder.Entity<Receipt>(entity =>
        {
            entity.HasKey(r => r.Id);                                        // Primärschlüssel = Id.
            entity.Property(r => r.ImagePath).IsRequired().HasMaxLength(500); // Pfad zum Belegbild: Pflichtfeld, max. 500 Zeichen.

            // Beziehung Receipt → Expense: zu welcher Ausgabe gehört der Beleg?
            entity.HasOne(r => r.Expense)
                .WithMany(e => e.Receipts)
                .HasForeignKey(r => r.ExpenseId)
                .OnDelete(DeleteBehavior.Cascade);                         // Cascade: Ausgabe gelöscht → ihre Belege werden mitgelöscht.
        });

        // ── ExchangeRate (Wechselkurs zwischen zwei Währungen) ──
        modelBuilder.Entity<ExchangeRate>(entity =>
        {
            entity.HasKey(er => er.Id);                                          // Primärschlüssel = Id.
            entity.Property(er => er.FromCurrency).IsRequired().HasMaxLength(3); // Ausgangswährung als 3-stelliger ISO-Code; Pflichtfeld.
            entity.Property(er => er.ToCurrency).IsRequired().HasMaxLength(3);   // Zielwährung als 3-stelliger ISO-Code; Pflichtfeld.
            entity.Property(er => er.Rate).HasPrecision(18, 6);                // Kurs als decimal(18,6): 6 Nachkommastellen für präzise Umrechnung.
            entity.HasIndex(er => new { er.FromCurrency, er.ToCurrency });     // Index (FromCurrency, ToCurrency): schnelles Finden des Kurses für ein Währungspaar.
        });

        // ── CountryCurrency (Zuordnung Land → Währung) ──
        modelBuilder.Entity<CountryCurrency>(entity =>
        {
            entity.HasKey(cc => cc.Id);                                          // Primärschlüssel = Id.
            entity.Property(cc => cc.Country).IsRequired().HasMaxLength(100);    // Ländername: Pflichtfeld, max. 100 Zeichen.
            entity.HasIndex(cc => cc.Country).IsUnique();                       // Unique-Index: jedes Land darf nur einmal vorkommen.
            entity.Property(cc => cc.CurrencyCode).IsRequired().HasMaxLength(3); // Währungscode als 3-stelliger ISO-Code; Pflichtfeld.
        });

        // ── Seed: 8 Standard-Ausgabenkategorien ──
        // HasData schreibt diese festen Datensätze über die Migration einmalig in die Tabelle,
        // sodass der User beim Erfassen einer Ausgabe sofort Kategorien auswählen kann (kein leeres Dropdown).
        // Die Ids sind fest vergeben, damit EF Core bei späteren Migrationen Änderungen erkennen kann.
        modelBuilder.Entity<ExpenseCategory>().HasData(
            new ExpenseCategory { Id = 1, Name = "Food", Icon = "🍔" },
            new ExpenseCategory { Id = 2, Name = "Night Out", Icon = "🍻" },
            new ExpenseCategory { Id = 3, Name = "Transport", Icon = "🚕" },
            new ExpenseCategory { Id = 4, Name = "Accommodation", Icon = "🏨" },
            new ExpenseCategory { Id = 5, Name = "Activities", Icon = "🎢" },
            new ExpenseCategory { Id = 6, Name = "Shopping", Icon = "🛍️" },
            new ExpenseCategory { Id = 7, Name = "Flights", Icon = "✈️" },
            new ExpenseCategory { Id = 8, Name = "Other", Icon = "📦" }
        );

        // ── Seed: 25 häufige Land-Währung-Zuordnungen ──
        // Vorbefüllte Daten, damit der User beim Anlegen eines Trips ein Land wählen kann und
        // die passende Währung automatisch vorgeschlagen wird. Auch hier feste Ids für stabile Migrationen.
        modelBuilder.Entity<CountryCurrency>().HasData(
            new CountryCurrency { Id = 1, Country = "Germany", CurrencyCode = "EUR" },
            new CountryCurrency { Id = 2, Country = "France", CurrencyCode = "EUR" },
            new CountryCurrency { Id = 3, Country = "Spain", CurrencyCode = "EUR" },
            new CountryCurrency { Id = 4, Country = "Italy", CurrencyCode = "EUR" },
            new CountryCurrency { Id = 5, Country = "Portugal", CurrencyCode = "EUR" },
            new CountryCurrency { Id = 6, Country = "Netherlands", CurrencyCode = "EUR" },
            new CountryCurrency { Id = 7, Country = "Austria", CurrencyCode = "EUR" },
            new CountryCurrency { Id = 8, Country = "Greece", CurrencyCode = "EUR" },
            new CountryCurrency { Id = 9, Country = "United States", CurrencyCode = "USD" },
            new CountryCurrency { Id = 10, Country = "United Kingdom", CurrencyCode = "GBP" },
            new CountryCurrency { Id = 11, Country = "Switzerland", CurrencyCode = "CHF" },
            new CountryCurrency { Id = 12, Country = "Japan", CurrencyCode = "JPY" },
            new CountryCurrency { Id = 13, Country = "Australia", CurrencyCode = "AUD" },
            new CountryCurrency { Id = 14, Country = "Canada", CurrencyCode = "CAD" },
            new CountryCurrency { Id = 15, Country = "Turkey", CurrencyCode = "TRY" },
            new CountryCurrency { Id = 16, Country = "Thailand", CurrencyCode = "THB" },
            new CountryCurrency { Id = 17, Country = "Mexico", CurrencyCode = "MXN" },
            new CountryCurrency { Id = 18, Country = "Brazil", CurrencyCode = "BRL" },
            new CountryCurrency { Id = 19, Country = "Croatia", CurrencyCode = "EUR" },
            new CountryCurrency { Id = 20, Country = "Czech Republic", CurrencyCode = "CZK" },
            new CountryCurrency { Id = 21, Country = "Hungary", CurrencyCode = "HUF" },
            new CountryCurrency { Id = 22, Country = "Poland", CurrencyCode = "PLN" },
            new CountryCurrency { Id = 23, Country = "Sweden", CurrencyCode = "SEK" },
            new CountryCurrency { Id = 24, Country = "Norway", CurrencyCode = "NOK" },
            new CountryCurrency { Id = 25, Country = "Denmark", CurrencyCode = "DKK" }
        );
    }
}
