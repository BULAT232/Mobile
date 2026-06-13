using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CheckersGame.Models;
using CheckersGame.Services;
using CheckersGame.Views;

namespace CheckersGame.ViewModels;

/// <summary>
/// ViewModel настройки новой игры.
/// </summary>
public partial class GameSetupViewModel : ObservableObject
{
    private readonly ProfileService _profileService;
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    private ObservableCollection<PlayerProfile> _profiles = new();

    [ObservableProperty]
    private PlayerProfile? _selectedPlayer1;

    [ObservableProperty]
    private PlayerProfile? _selectedPlayer2;

    [ObservableProperty]
    private int _selectedGameTypeIndex;

    [ObservableProperty]
    private bool _enableTimer;

    [ObservableProperty]
    private int _timerMinutes = 10;

    public List<string> GameTypeNames { get; } = new()
    {
        "Русские шашки (8×8)",
        "Международные шашки (10×10)",
        "Бразильские шашки (8×8)",
        "Поддавки (8×8)"
    };

    public List<int> TimerOptions { get; } = new() { 3, 5, 10, 15, 20, 30 };

    public GameSetupViewModel(ProfileService profileService, SettingsService settingsService)
    {
        _profileService = profileService;
        _settingsService = settingsService;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        var profiles = await _profileService.GetProfilesAsync();
        Profiles = new ObservableCollection<PlayerProfile>(profiles);

        var settings = await _settingsService.GetSettingsAsync();
        EnableTimer = settings.EnableTimer;
        TimerMinutes = settings.DefaultTimerMinutes;

        if (Profiles.Count >= 2)
        {
            SelectedPlayer1 = Profiles[0];
            SelectedPlayer2 = Profiles[1];
        }
        else if (Profiles.Count == 1)
        {
            SelectedPlayer1 = Profiles[0];
        }
    }

    [RelayCommand]
    private async Task StartGameAsync()
    {
        if (SelectedPlayer1 == null || SelectedPlayer2 == null)
        {
            await Shell.Current.DisplayAlert("Ошибка", "Выберите обоих игроков", "OK");
            return;
        }

        if (SelectedPlayer1.Id == SelectedPlayer2.Id)
        {
            await Shell.Current.DisplayAlert("Ошибка", "Выберите разных игроков", "OK");
            return;
        }

        var gameType = (GameType)SelectedGameTypeIndex;

        var navigationParams = new Dictionary<string, object>
        {
            { "GameType", gameType },
            { "Player1", SelectedPlayer1 },
            { "Player2", SelectedPlayer2 },
            { "EnableTimer", EnableTimer },
            { "TimerMinutes", TimerMinutes }
        };

        await Shell.Current.GoToAsync(nameof(GamePage), navigationParams);
    }

    [RelayCommand]
    private async Task CreateQuickProfileAsync()
    {
        string? name = await Shell.Current.DisplayPromptAsync(
            "Новый профиль", "Введите имя игрока:", "Создать", "Отмена");

        if (!string.IsNullOrWhiteSpace(name))
        {
            var profile = await _profileService.CreateProfileAsync(name);
            Profiles.Add(profile);

            if (SelectedPlayer1 == null)
                SelectedPlayer1 = profile;
            else if (SelectedPlayer2 == null)
                SelectedPlayer2 = profile;
        }
    }
}
