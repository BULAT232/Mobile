using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CheckersGame.Services;
using CheckersGame.Views;

namespace CheckersGame.ViewModels;

/// <summary>
/// ViewModel главного меню.
/// </summary>
public partial class MainMenuViewModel : ObservableObject
{
    private readonly ProfileService _profileService;
    private readonly StatisticsService _statisticsService;

    [ObservableProperty]
    private int _totalGames;

    [ObservableProperty]
    private int _totalProfiles;

    public MainMenuViewModel(ProfileService profileService, StatisticsService statisticsService)
    {
        _profileService = profileService;
        _statisticsService = statisticsService;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        var profiles = await _profileService.GetProfilesAsync();
        TotalProfiles = profiles.Count;

        var stats = await _statisticsService.GetOverallStatsAsync();
        TotalGames = stats.TotalGames;
    }

    [RelayCommand]
    private async Task NavigateToNewGameAsync()
    {
        await Shell.Current.GoToAsync(nameof(GameSetupPage));
    }

    [RelayCommand]
    private async Task NavigateToProfilesAsync()
    {
        await Shell.Current.GoToAsync(nameof(ProfilesPage));
    }

    [RelayCommand]
    private async Task NavigateToStatisticsAsync()
    {
        await Shell.Current.GoToAsync(nameof(StatisticsPage));
    }

    [RelayCommand]
    private async Task NavigateToHistoryAsync()
    {
        await Shell.Current.GoToAsync(nameof(GameHistoryPage));
    }

    [RelayCommand]
    private async Task NavigateToSettingsAsync()
    {
        await Shell.Current.GoToAsync(nameof(SettingsPage));
    }
}
