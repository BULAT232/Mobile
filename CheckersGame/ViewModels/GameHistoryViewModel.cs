using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CheckersGame.Models;
using CheckersGame.Services;

namespace CheckersGame.ViewModels;

/// <summary>
/// ViewModel страницы истории игр.
/// </summary>
public partial class GameHistoryViewModel : ObservableObject
{
    private readonly StatisticsService _statisticsService;

    [ObservableProperty]
    private ObservableCollection<GameRecord> _gameRecords = new();

    [ObservableProperty]
    private bool _isEmpty = true;

    public GameHistoryViewModel(StatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    [RelayCommand]
    private async Task LoadHistoryAsync()
    {
        var records = await _statisticsService.GetAllRecordsAsync();
        GameRecords = new ObservableCollection<GameRecord>(
            records.OrderByDescending(r => r.Date));
        IsEmpty = GameRecords.Count == 0;
    }

    [RelayCommand]
    private async Task ClearHistoryAsync()
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "Очистить историю",
            "Удалить все записи об играх? Это действие нельзя отменить.",
            "Удалить", "Отмена");

        if (confirm)
        {
            await _statisticsService.ClearAllRecordsAsync();
            GameRecords.Clear();
            IsEmpty = true;
        }
    }

    [RelayCommand]
    private async Task ShowGameDetailsAsync(GameRecord record)
    {
        string details = $"Дата: {record.Date:dd.MM.yyyy HH:mm}\n" +
                         $"Тип: {record.GameTypeDisplay}\n" +
                         $"Игрок 1: {record.Player1Name}\n" +
                         $"Игрок 2: {record.Player2Name}\n" +
                         $"Результат: {record.ResultDisplay}\n" +
                         $"Ходов: {record.MoveCount}\n" +
                         $"Длительность: {record.DurationDisplay}\n" +
                         $"Захвачено (И1): {record.Player1Captured}\n" +
                         $"Захвачено (И2): {record.Player2Captured}";

        await Shell.Current.DisplayAlert("Детали игры", details, "OK");
    }
}
