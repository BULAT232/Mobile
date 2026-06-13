using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TypingTrainer.Models;
using TypingTrainer.Services;
using TypingTrainer.Views;

namespace TypingTrainer.ViewModels;

/// <summary>
/// ViewModel for the main page where user selects a dictionary and starts the game.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly DictionaryService _dictionaryService;
    private readonly StatisticsService _statisticsService;
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    private ObservableCollection<TypingDictionary> _dictionaries = new();

    [ObservableProperty]
    private TypingDictionary? _selectedDictionary;

    [ObservableProperty]
    private double _bestWPM;

    [ObservableProperty]
    private double _averageWPM;

    [ObservableProperty]
    private int _totalSessions;

    [ObservableProperty]
    private bool _isLoading;

    public MainViewModel(DictionaryService dictionaryService, StatisticsService statisticsService, SettingsService settingsService)
    {
        _dictionaryService = dictionaryService;
        _statisticsService = statisticsService;
        _settingsService = settingsService;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var dicts = await _dictionaryService.GetAllDictionariesAsync();
            Dictionaries = new ObservableCollection<TypingDictionary>(dicts);

            BestWPM = await _statisticsService.GetBestWPMAsync();
            AverageWPM = await _statisticsService.GetAverageWPMAsync();
            TotalSessions = await _statisticsService.GetTotalSessionsAsync();

            var settings = await _settingsService.GetSettingsAsync();
            if (!string.IsNullOrEmpty(settings.SelectedDictionaryId))
            {
                SelectedDictionary = Dictionaries.FirstOrDefault(d => d.Id == settings.SelectedDictionaryId);
            }

            if (SelectedDictionary == null && Dictionaries.Count > 0)
            {
                SelectedDictionary = Dictionaries[0];
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task StartGameAsync()
    {
        if (SelectedDictionary == null)
        {
            await Shell.Current.DisplayAlert("Ошибка", "Выберите словарь для начала игры", "OK");
            return;
        }

        var settings = await _settingsService.GetSettingsAsync();
        settings.SelectedDictionaryId = SelectedDictionary.Id;
        await _settingsService.SaveSettingsAsync(settings);

        await Shell.Current.GoToAsync($"{nameof(GamePage)}?dictionaryId={SelectedDictionary.Id}");
    }

    [RelayCommand]
    private async Task NavigateToDictionariesAsync()
    {
        await Shell.Current.GoToAsync("//DictionaryListPage");
    }

    [RelayCommand]
    private async Task NavigateToStatisticsAsync()
    {
        await Shell.Current.GoToAsync("//StatisticsPage");
    }

    [RelayCommand]
    private async Task NavigateToSettingsAsync()
    {
        await Shell.Current.GoToAsync("//SettingsPage");
    }
}
