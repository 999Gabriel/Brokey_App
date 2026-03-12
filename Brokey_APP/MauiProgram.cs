﻿using Brokey_APP.Services;
using Brokey_APP.ViewModels;
using Brokey_APP.Views;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace Brokey_APP;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Poppins-Regular.ttf", "PoppinsRegular");
                fonts.AddFont("Poppins-SemiBold.ttf", "PoppinsSemiBold");
                fonts.AddFont("Poppins-Bold.ttf", "PoppinsBold");
                fonts.AddFont("Pacifico-Regular.ttf", "Pacifico");
            });

        // ── Services ──
        builder.Services.AddSingleton<ITokenStorageService, TokenStorageService>();
        builder.Services.AddTransient<AuthHttpMessageHandler>();

        builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
        {
            client.BaseAddress = ApiConfig.BaseUri;
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();
#if DEBUG
            // Allow self-signed certs in dev
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
#endif
            return handler;
        })
        .AddHttpMessageHandler<AuthHttpMessageHandler>();

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
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<TripsViewModel>();
        builder.Services.AddTransient<CreateTripViewModel>();
        builder.Services.AddTransient<TripDetailViewModel>();
        builder.Services.AddTransient<GroupDetailViewModel>();
        builder.Services.AddTransient<AddMemberViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<AboutViewModel>();

        // ── Views ──
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<TripsPage>();
        builder.Services.AddTransient<CreateTripPage>();
        builder.Services.AddTransient<TripDetailPage>();
        builder.Services.AddTransient<GroupDetailPage>();
        builder.Services.AddTransient<AddMemberPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<AboutPage>();
        builder.Services.AddTransient<ImpressumPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
