using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ORM;

/// <summary>
/// Factory used by 'dotnet ef' CLI to create the DbContext at design time for migrations.
/// </summary>
// Diese Factory wird NUR zur ENTWURFSZEIT (design time) gebraucht, NICHT wenn die App läuft.
// Hintergrund: Beim Befehl "dotnet ef migrations add ..." bzw. "dotnet ef database update" läuft
// keine richtige App (kein Program.cs mit Dependency Injection). Die EF-Core-Tools brauchen aber
// trotzdem einen AppDbContext, um das Modell zu lesen und Migrationen zu erzeugen/anzuwenden.
// EF Core sucht automatisch nach einer Klasse, die IDesignTimeDbContextFactory<AppDbContext> implementiert,
// und ruft deren CreateDbContext(...) auf, um sich einen DbContext zu "basteln".
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    // Wird von den EF-Core-Tools aufgerufen und liefert einen fertig konfigurierten AppDbContext zurück.
    // 'args' sind eventuelle Kommandozeilen-Argumente der CLI (hier nicht verwendet).
    public AppDbContext CreateDbContext(string[] args)
    {
        // Fest hinterlegter Connection-String zur lokalen MySQL-Datenbank "Brokey".
        // Bewusst hier hardcodiert, weil zur Entwurfszeit keine appsettings.json/DI-Konfiguration geladen wird.
        // (Zur Laufzeit kommt der echte Connection-String dagegen aus Program.cs/appsettings.)
        var connectionString = "Server=localhost;Database=Brokey;User=root;Password=Nij43Bq8;";

        // optionsBuilder baut die DbContextOptions zusammen (entspricht dem, was sonst DI macht).
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseMySQL(connectionString); // Legt MySQL als Datenbank-Provider fest.

        // Erzeugt den AppDbContext mit den eben gebauten Optionen und gibt ihn an die EF-Tools zurück.
        return new AppDbContext(optionsBuilder.Options);
    }
}

