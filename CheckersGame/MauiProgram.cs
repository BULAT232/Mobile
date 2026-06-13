using Microsoft.Extensions.Logging;
using CheckersGame.Services;
using CheckersGame.ViewModels;
using CheckersGame.Views;

namespace CheckersGame;

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

        // Services
        builder.Services.AddSingleton<ProfileService>();
        builder.Services.AddSingleton<StatisticsService>();
        builder.Services.AddSingleton<SettingsService>();
        builder.Services.AddTransient<GameEngine>();

        // ViewModels
        builder.Services.AddTransient<MainMenuViewModel>();
        builder.Services.AddTransient<GameSetupViewModel>();
        builder.Services.AddTransient<GameViewModel>();
        builder.Services.AddTransient<ProfilesViewModel>();
        builder.Services.AddTransient<StatisticsViewModel>();
        builder.Services.AddTransient<GameHistoryViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        // Pages
        builder.Services.AddTransient<MainMenuPage>();
        builder.Services.AddTransient<GameSetupPage>();
        builder.Services.AddTransient<GamePage>();
        builder.Services.AddTransient<ProfilesPage>();
        builder.Services.AddTransient<StatisticsPage>();
        builder.Services.AddTransient<GameHistoryPage>();
        builder.Services.AddTransient<SettingsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
