using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TypingTrainer.Models;
using TypingTrainer.Services;

namespace TypingTrainer.ViewModels;

/// <summary>
/// ViewModel for the statistics page.
/// </summary>
public partial class StatisticsViewModel : ObservableObject
{
    private readonly StatisticsService _statisticsService;

    [ObservableProperty]
    private double _bestWPM;

    [ObservableProperty]
    private double _averageWPM;

    [ObservableProperty]
    private double _averageAccuracy;

    [ObservableProperty]
    private int _totalSessions;

    [ObservableProperty]
    private double _totalPracticeMinutes;

    [ObservableProperty]
    private ObservableCollection<SessionResult> _recentResults = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasData;

    public StatisticsViewModel(StatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    [RelayCommand]
    private async Task LoadStatisticsAsync()
    {
        IsLoading = true;
        try
        {
            BestWPM = Math.Round(await _statisticsService.GetBestWPMAsync(), 1);
            AverageWPM = Math.Round(await _statisticsService.GetAverageWPMAsync(), 1);
            AverageAccuracy = Math.Round(await _statisticsService.GetAverageAccuracyAsync(), 1);
            TotalSessions = await _statisticsService.GetTotalSessionsAsync();
            TotalPracticeMinutes = Math.Round(await _statisticsService.GetTotalPracticeTimeMinutesAsync(), 1);

            var recent = await _statisticsService.GetRecentResultsAsync(20);
            RecentResults = new ObservableCollection<SessionResult>(recent);

            HasData = TotalSessions > 0;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ClearStatisticsAsync()
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "Очистка статистики",
            "Вы уверены, что хотите удалить всю статистику? Это действие нельзя отменить.",
            "Удалить",
            "Отмена");

        if (confirm)
        {
            await _statisticsService.ClearAllAsync();
            await LoadStatisticsAsync();
        }
    }
}
