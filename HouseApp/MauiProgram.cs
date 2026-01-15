using HouseApp.Services;
using HouseApp.ViewModels;
using HouseApp.Views;

namespace HouseApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // --------------------
        // Services
        // --------------------
        builder.Services.AddSingleton<UserSession>();

        // --------------------
        // ViewModels
        // --------------------
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddSingleton<MainViewModel>();

        // --------------------
        // Views (Pages)
        // --------------------
        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<HomeViewModel>();
        builder.Services.AddSingleton<HomePage>();
        builder.Services.AddSingleton<CalendarPage>();

        return builder.Build();
    }
}
