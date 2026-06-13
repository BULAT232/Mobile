using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CheckersGame.Models;
using CheckersGame.Services;

namespace CheckersGame.ViewModels;

/// <summary>
/// ViewModel страницы статистики.
/// </summary>
public partial class StatisticsViewModel : ObservableObject
{
    private readonly StatisticsService _statisticsService;
    private readonly ProfileService _profileService;

    [ObservableProperty]
    private int _totalGames;

    [ObservableProperty]
    private string _totalDuration = "00:00";

    [ObservableProperty]
    private string _averageDuration = "00:00";

    [ObservableProperty]
    private int _totalMoves;

    [ObservableProperty]
    private int _drawCount;

    [ObservableProperty]
    private int _russianGames;

    [ObservableProperty]
    private int _internationalGames;

    [ObservableProperty]
    private int _brazilianGames;

    [ObservableProperty]
    private int _giveawayGames;

    [ObservableProperty]
    private ObservableCollection<PlayerProfile> _playerRankings = new();

    [ObservableProperty]
    private PlayerProfile? _selectedPlayer;

    [ObservableProperty]
    private string _playerWins = "0";

    [ObservableProperty]
    private string _playerLosses = "0";

    [ObservableProperty]
    private string _playerDraws = "0";

    [ObservableProperty]
    private string _playerWinRate = "0%";

    [ObservableProperty]
    private string _playerTotalCaptured = "0";

    [ObservableProperty]
    private string _playerBestStreak = "0";

    [ObservableProperty]
    private bool _hasPlayerSelected;

    public StatisticsViewModel(StatisticsService statisticsService, ProfileService profileService)
    {
        _statisticsService = statisticsService;
        _profileService = profileService;
    }

    [RelayCommand]
    private async Task LoadStatsAsync()
    {
        var stats = await _statisticsService.GetOverallStatsAsync();

        TotalGames = stats.TotalGames;
        TotalDuration = FormatDuration(stats.TotalDuration);
        AverageDuration = FormatDuration(stats.AverageDuration);
        TotalMoves = stats.TotalMoves;
        DrawCount = stats.DrawCount;

        RussianGames = stats.GamesByType.GetValueOrDefault(GameType.Russian);
        InternationalGames = stats.GamesByType.GetValueOrDefault(GameType.International);
        BrazilianGames = stats.GamesByType.GetValueOrDefault(GameType.Brazilian);
        GiveawayGames = stats.GamesByType.GetValueOrDefault(GameType.Giveaway);

        var profiles = await _profileService.GetProfilesAsync();
        PlayerRankings = new ObservableCollection<PlayerProfile>(
            profiles.OrderByDescending(p => p.Rating));
    }

    [RelayCommand]
    private async Task SelectPlayerAsync(PlayerProfile player)
    {
        SelectedPlayer = player;
        HasPlayerSelected = true;

        var playerStats = await _statisticsService.GetPlayerStatsAsync(player.Id);
        PlayerWins = playerStats.Wins.ToString();
        PlayerLosses = playerStats.Losses.ToString();
        PlayerDraws = playerStats.Draws.ToString();
        PlayerWinRate = $"{playerStats.WinRate:F1}%";
        PlayerTotalCaptured = player.TotalCaptured.ToString();
        PlayerBestStreak = player.BestStreak.ToString();
    }

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
            return duration.ToString(@"h\:mm\:ss");
        return duration.ToString(@"mm\:ss");
    }
}
