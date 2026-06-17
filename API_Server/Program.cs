using System.Text;
using API_Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ORM;
using ORM.Repositories;

// Einstiegspunkt der gesamten API. Hier wird die Anwendung KONFIGURIERT (Services im DI-Container registrieren)
// und danach die MIDDLEWARE-PIPELINE aufgebaut, die jeden eingehenden HTTP-Request durchläuft.
// builder kapselt Konfiguration (appsettings.json), DI-Container und Web-Host.
var builder = WebApplication.CreateBuilder(args);

// ── Database (MySQL via Oracle MySql.EntityFrameworkCore) ──
// Verbindungsstring "DefaultConnection" aus appsettings.json lesen ("!" = wir garantieren, dass er existiert).
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
// AppDbContext (aus dem ORM-Projekt) im DI-Container registrieren und auf MySQL konfigurieren.
// Dadurch kann jeder Controller/jedes Repository einen DbContext per Konstruktor injiziert bekommen.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(connectionString));

// ── JWT Authentication ──
// Den "Jwt"-Abschnitt aus appsettings.json laden (enthält Key, Issuer, Audience, ExpireHours).
var jwtSettings = builder.Configuration.GetSection("Jwt");
// Den geheimen Signatur-Schlüssel als Byte-Array. Mit GENAU diesem Key signiert der TokenService die Tokens,
// und mit demselben Key prüft die API hier eingehende Tokens → nur damit signierte Tokens werden akzeptiert.
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
    {
        // Standard-Verfahren = JWT-Bearer: Tokens werden aus dem "Authorization: Bearer <token>"-Header gelesen.
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // TokenValidationParameters legen fest, WAS bei jedem eingehenden Token geprüft wird:
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,           // prüft: stammt das Token vom erwarteten Aussteller (Issuer)?
            ValidateAudience = true,         // prüft: ist das Token für diese API (Audience) bestimmt?
            ValidateLifetime = true,         // prüft: ist das Token noch gültig (nicht abgelaufen)?
            ValidateIssuerSigningKey = true, // prüft: wurde das Token mit unserem geheimen Key signiert (nicht manipuliert)?
            ValidIssuer = jwtSettings["Issuer"],          // erwarteter Issuer-Wert (Soll-Wert für die Prüfung oben)
            ValidAudience = jwtSettings["Audience"],      // erwartete Audience
            IssuerSigningKey = new SymmetricSecurityKey(key),  // der Schlüssel, gegen den die Signatur verifiziert wird
            ClockSkew = TimeSpan.FromMinutes(1)  // erlaubte Uhren-Abweichung zwischen Server/Client (Standard wären 5 Min)
        };
    });

// Aktiviert die Autorisierung; nötig, damit [Authorize] auf Controllern/Actions ausgewertet wird.
builder.Services.AddAuthorization();

// ── Services ──
// DI-Registrierungen. AddScoped = eine Instanz PRO HTTP-Request (passt zum DbContext, der ebenfalls scoped ist).
builder.Services.AddScoped<TokenService>();           // JWT-Erzeugung
builder.Services.AddScoped<TripRepository>();          // DB-Queries rund um Trips
builder.Services.AddScoped<GroupRepository>();         // DB-Queries rund um Gruppen
builder.Services.AddScoped<GroupMemberRepository>();   // DB-Queries rund um Gruppenmitglieder
builder.Services.AddScoped<TripMemberRepository>();    // DB-Queries rund um Trip-Teilnehmer
builder.Services.AddScoped<ExpenseRepository>();       // DB-Queries rund um Ausgaben/Splits

// ── Controllers + Swagger ──
builder.Services.AddControllers();          // findet und registriert alle [ApiController]-Klassen
builder.Services.AddEndpointsApiExplorer(); // Metadaten der Endpunkte für OpenAPI/Swagger
builder.Services.AddOpenApi();              // OpenAPI-Dokument (.NET 9/10)
builder.Services.AddSwaggerGen();           // generiert die interaktive Swagger-UI zum Testen der API

// ── CORS (allow MAUI app requests) ──
// CORS-Policy "AllowAll": erlaubt Cross-Origin-Requests von beliebigen Quellen/Methoden/Headern.
// Für Entwicklung praktisch (MAUI-Client greift von anderem Origin zu); für Produktion zu offen.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Ab hier ist die Konfiguration abgeschlossen: build() erzeugt die fertige App.
var app = builder.Build();

// ── Middleware pipeline ──
// Nur in der Entwicklung: OpenAPI-Dokument + Swagger-UI bereitstellen (nicht in Produktion sichtbar).
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Aufruf der Wurzel "/" leitet auf die Swagger-UI um (ExcludeFromDescription = taucht nicht in der API-Doku auf).
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

// WICHTIG: Die REIHENFOLGE der Middleware bestimmt, in welcher Abfolge jeder Request verarbeitet wird:
app.UseHttpsRedirection();  // 1) HTTP → HTTPS umleiten (Tokens dürfen nie unverschlüsselt übertragen werden)
app.UseCors("AllowAll");    // 2) CORS-Header setzen, BEVOR Auth läuft, sonst fehlen sie bei abgelehnten Requests
app.UseAuthentication();    // 3) Token aus dem Bearer-Header lesen/prüfen und User.Identity füllen (WER ist es?)
app.UseAuthorization();     // 4) [Authorize] auswerten – setzt Authentication voraus (DARF die Person das?)
app.MapControllers();       // 5) Request an die passende Controller-Action weiterleiten

app.Run();  // Startet den Webserver und blockiert, bis die App beendet wird.
