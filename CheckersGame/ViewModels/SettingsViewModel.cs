using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CheckersGame.Models;
using CheckersGame.Services;

namespace CheckersGame.ViewModels;

/// <summary>
/// ViewModel страницы настроек.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    private int _selectedThemeIndex;

    [ObservableProperty]
    private bool _showHints = true;

    [ObservableProperty]
    private bool _enableTimer;

    [ObservableProperty]
    private int _defaultTimerMinutes = 10;

    [ObservableProperty]
    private bool _enableSound = true;

    public List<string> ThemeNames { get; } = new()
    {
        "Классическая (зелёная)",
        "Деревянная (коричневая)",
        "Тёмная"
    };

    public List<int> TimerOptions { get; } = new() { 3, 5, 10, 15, 20, 30 };

    public SettingsViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [RelayCommand]
    private async Task LoadSettingsAsync()
    {
        var settings = await _settingsService.GetSettingsAsync();
        SelectedThemeIndex = (int)settings.BoardTheme;
        ShowHints = settings.ShowHints;
        EnableTimer = settings.EnableTimer;
        DefaultTimerMinutes = settings.DefaultTimerMinutes;
        EnableSound = settings.EnableSound;
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        var settings = new AppSettings
        {
            BoardTheme = (BoardTheme)SelectedThemeIndex,
            ShowHints = ShowHints,
            EnableTimer = EnableTimer,
            DefaultTimerMinutes = DefaultTimerMinutes,
            EnableSound = EnableSound
        };

        await _settingsService.SaveSettingsAsync(settings);
        await Shell.Current.DisplayAlert("Настройки", "Настройки сохранены", "OK");
    }

    [RelayCommand]
    private async Task ResetSettingsAsync()
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "Сброс настроек",
            "Сбросить все настройки по умолчанию?",
            "Сбросить", "Отмена");

        if (confirm)
        {
            await _settingsService.ResetToDefaultsAsync();
            await LoadSettingsAsync();
        }
    }
}
