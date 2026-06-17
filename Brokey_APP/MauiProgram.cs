using Brokey_APP.Services;
using Brokey_APP.ViewModels;
using Brokey_APP.Views;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace Brokey_APP;

// Zentrale Konfigurationsklasse der MAUI-App. Hier wird der Dependency-Injection-Container (DI) befüllt:
// DI bedeutet, dass Klassen (z. B. Seiten) ihre Abhängigkeiten – etwa ViewModels oder Services – nicht selbst
// erzeugen, sondern im Konstruktor übergeben bekommen. Alles, was injiziert werden soll, muss hier registriert sein.
public static class MauiProgram
{
    // Baut und konfiguriert die App: registriert Plugins (CommunityToolkit, Maps), Schriftarten,
    // DI-Dienste, ViewModels und Views und liefert die fertige MauiApp zurück.
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        // Kern-App, CommunityToolkit und Karten-Unterstützung aktivieren und Schriftarten laden.
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiMaps()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Charter Regular.ttf", "Charter");
                fonts.AddFont("Charter Bold.ttf", "CharterBold");
                fonts.AddFont("Charter Italic.ttf", "CharterItalic");
                fonts.AddFont("Charter Bold Italic.ttf", "CharterBoldItalic");
                fonts.AddFont("Inter-Regular.ttf", "Inter");
                fonts.AddFont("Inter-Medium.ttf", "InterMedium");
                fonts.AddFont("Inter-SemiBold.ttf", "InterSemiBold");
                fonts.AddFont("Inter-Italic.ttf", "InterItalic");
                fonts.AddFont("Inter-Bold.ttf", "InterBold");
                fonts.AddFont("jetbrains-mono.regular.ttf", "JetBrainsMono");
                fonts.AddFont("jetbrains-mono.bold.ttf", "JetBrainsMonoBold");
                fonts.AddFont("jetbrains-mono.italic.ttf", "JetBrainsMonoItalic");
                // Keep legacy aliases alive so any remaining hard-coded references still resolve.
                fonts.AddFont("Inter-Regular.ttf", "PoppinsRegular");
                fonts.AddFont("Inter-SemiBold.ttf", "PoppinsSemiBold");
                fonts.AddFont("Poppins-Bold.ttf", "PoppinsBold");
                fonts.AddFont("Pacifico-Regular.ttf", "Pacifico");
            });

        // ── Services ──
        // Singleton = genau eine Instanz für die gesamte App-Laufzeit: das Token muss überall identisch sein.
        builder.Services.AddSingleton<ITokenStorageService, TokenStorageService>();
        // Transient = bei jeder Anfrage eine neue Instanz: dieser Handler hängt das Bearer-Token an jede HTTP-Anfrage.
        builder.Services.AddTransient<AuthHttpMessageHandler>();

        // Typisierter HttpClient für AuthService: Basis-URL + JSON-Header, in DEBUG self-signed Zertifikate erlauben,
        // und den AuthHttpMessageHandler einhängen (fügt das Bearer-Token an).
        builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
        {
            client.BaseAddress = ApiConfig.BaseUri;
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            // Innerster HTTP-Handler, der die eigentliche Netzwerkverbindung herstellt.
            var handler = new HttpClientHandler();
#if DEBUG
            // Nur im DEBUG-Build: akzeptiert selbst-signierte HTTPS-Zertifikate des lokalen Dev-Servers
            // (im Release wäre das ein Sicherheitsrisiko und wird daher wegkompiliert).
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
#endif
            return handler;
        })
        // Hängt den AuthHttpMessageHandler vor jede Anfrage → fügt automatisch "Authorization: Bearer <Token>" hinzu.
        .AddHttpMessageHandler<AuthHttpMessageHandler>();

        // Typisierter HttpClient für TripService: gleiche Konfiguration wie oben (Basis-URL, JSON, Auth-Token).
        builder.Services.AddHttpClient<ITripService, TripService>(client =>
        {
            client.BaseAddress = ApiConfig.BaseUri;
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();
#if DEBUG
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
#endif
            return handler;
        })
        .AddHttpMessageHandler<AuthHttpMessageHandler>();

        // ── ViewModels ──
        // Alle ViewModels als Transient registrieren, damit jede Seite eine frische Instanz bekommt.
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<TripsViewModel>();
        builder.Services.AddTransient<CreateTripViewModel>();
        builder.Services.AddTransient<TripDetailViewModel>();
        builder.Services.AddTransient<GroupDetailViewModel>();
        builder.Services.AddTransient<AddMemberViewModel>();
        builder.Services.AddTransient<AddExpenseViewModel>();
        builder.Services.AddTransient<ExpenseDetailViewModel>();
        builder.Services.AddTransient<TripSummaryViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<AboutViewModel>();

        // ── Views ──
        // Alle Seiten als Transient registrieren; der jeweilige ViewModel wird per Konstruktor injiziert.
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<TripsPage>();
        builder.Services.AddTransient<CreateTripPage>();
        builder.Services.AddTransient<TripDetailPage>();
        builder.Services.AddTransient<GroupDetailPage>();
        builder.Services.AddTransient<AddMemberPage>();
        builder.Services.AddTransient<AddExpensePage>();
        builder.Services.AddTransient<ExpenseDetailPage>();
        builder.Services.AddTransient<TripSummaryPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<AboutPage>();
        builder.Services.AddTransient<ImpressumPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
