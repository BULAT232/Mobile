using CheckersGame.Views;

namespace CheckersGame;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Регистрация маршрутов для навигации
        Routing.RegisterRoute(nameof(GameSetupPage), typeof(GameSetupPage));
        Routing.RegisterRoute(nameof(GamePage), typeof(GamePage));
        Routing.RegisterRoute(nameof(ProfilesPage), typeof(ProfilesPage));
        Routing.RegisterRoute(nameof(StatisticsPage), typeof(StatisticsPage));
        Routing.RegisterRoute(nameof(GameHistoryPage), typeof(GameHistoryPage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
    }
}
