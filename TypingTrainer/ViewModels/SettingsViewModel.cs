using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TypingTrainer.Models;
using TypingTrainer.Services;

namespace TypingTrainer.ViewModels;

/// <summary>
/// ViewModel for the settings page.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;
    private AppSettings? _settings;

    [ObservableProperty]
    private int _fontSize = 20;

    [ObservableProperty]
    private int _wordCount = 20;

    [ObservableProperty]
    private bool _showTimer = true;

    [ObservableProperty]
    private bool _showWPM = true;

    [ObservableProperty]
    private bool _showAccuracy = true;

    [ObservableProperty]
    private bool _highlightErrors = true;

    [ObservableProperty]
    private bool _soundEnabled = true;

    [ObservableProperty]
    private string _previewText = "Пример текста для набора";

    [ObservableProperty]
    private bool _isLoading;

    public SettingsViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [RelayCommand]
    private async Task LoadSettingsAsync()
    {
        IsLoading = true;
        try
        {
            _settings = await _settingsService.GetSettingsAsync();
            FontSize = _settings.FontSize;
            WordCount = _settings.WordCount;
            ShowTimer = _settings.ShowTimer;
            ShowWPM = _settings.ShowWPM;
            ShowAccuracy = _settings.ShowAccuracy;
            HighlightErrors = _settings.HighlightErrors;
            SoundEnabled = _settings.SoundEnabled;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        if (_settings == null) return;

        _settings.FontSize = FontSize;
        _settings.WordCount = WordCount;
        _settings.ShowTimer = ShowTimer;
        _settings.ShowWPM = ShowWPM;
        _settings.ShowAccuracy = ShowAccuracy;
        _settings.HighlightErrors = HighlightErrors;
        _settings.SoundEnabled = SoundEnabled;

        await _settingsService.SaveSettingsAsync(_settings);
        await Shell.Current.DisplayAlert("Успех", "Настройки сохранены", "OK");
    }

    [RelayCommand]
    private async Task ResetSettingsAsync()
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "Сброс настроек",
            "Вернуть все настройки к значениям по умолчанию?",
            "Сбросить",
            "Отмена");

        if (confirm)
        {
            await _settingsService.ResetToDefaultsAsync();
            await LoadSettingsAsync();
        }
    }

    partial void OnFontSizeChanged(int value)
    {
        // Clamp font size between 12 and 36
        if (value < 12) FontSize = 12;
        if (value > 36) FontSize = 36;
    }

    partial void OnWordCountChanged(int value)
    {
        // Clamp word count between 5 and 100
        if (value < 5) WordCount = 5;
        if (value > 100) WordCount = 100;
    }
}
