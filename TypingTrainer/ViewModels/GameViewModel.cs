using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TypingTrainer.Models;
using TypingTrainer.Services;

namespace TypingTrainer.ViewModels;

/// <summary>
/// ViewModel for the typing game page. Handles real-time typing tracking,
/// character-by-character comparison, timer, and statistics calculation.
/// </summary>
[QueryProperty(nameof(DictionaryId), "dictionaryId")]
public partial class GameViewModel : ObservableObject
{
    private readonly DictionaryService _dictionaryService;
    private readonly StatisticsService _statisticsService;
    private readonly SettingsService _settingsService;
    private System.Timers.Timer? _timer;
    private GameSession _session = new();

    [ObservableProperty]
    private string _dictionaryId = string.Empty;

    [ObservableProperty]
    private string _dictionaryName = string.Empty;

    [ObservableProperty]
    private string _targetText = string.Empty;

    [ObservableProperty]
    private string _typedText = string.Empty;

    [ObservableProperty]
    private string _elapsedTime = "00:00";

    [ObservableProperty]
    private double _currentWPM;

    [ObservableProperty]
    private double _currentCPM;

    [ObservableProperty]
    private double _currentAccuracy = 100;

    [ObservableProperty]
    private int _correctCount;

    [ObservableProperty]
    private int _errorCount;

    [ObservableProperty]
    private int _currentPosition;

    [ObservableProperty]
    private bool _isGameActive;

    [ObservableProperty]
    private bool _isGameCompleted;

    [ObservableProperty]
    private bool _isGameNotStarted = true;

    [ObservableProperty]
    private int _fontSize = 20;

    // For character-by-character display
    [ObservableProperty]
    private List<CharacterState> _characterStates = new();

    [ObservableProperty]
    private double _progressValue;

    // Result fields
    [ObservableProperty]
    private double _resultWPM;

    [ObservableProperty]
    private double _resultCPM;

    [ObservableProperty]
    private double _resultAccuracy;

    [ObservableProperty]
    private string _resultTime = string.Empty;

    [ObservableProperty]
    private int _resultErrors;

    public GameViewModel(DictionaryService dictionaryService, StatisticsService statisticsService, SettingsService settingsService)
    {
        _dictionaryService = dictionaryService;
        _statisticsService = statisticsService;
        _settingsService = settingsService;
    }

    [RelayCommand]
    private async Task InitializeGameAsync()
    {
        var settings = await _settingsService.GetSettingsAsync();
        FontSize = settings.FontSize;

        var dictionary = await _dictionaryService.GetDictionaryByIdAsync(DictionaryId);
        if (dictionary == null)
        {
            await Shell.Current.DisplayAlert("Ошибка", "Словарь не найден", "OK");
            await Shell.Current.GoToAsync("..");
            return;
        }

        DictionaryName = dictionary.Name;
        TargetText = dictionary.GenerateText(settings.WordCount);

        // Initialize character states
        CharacterStates = TargetText.Select(c => new CharacterState
        {
            Character = c,
            State = CharState.Pending
        }).ToList();

        _session = new GameSession
        {
            DictionaryId = dictionary.Id,
            DictionaryName = dictionary.Name,
            TargetText = TargetText
        };

        TypedText = string.Empty;
        CurrentPosition = 0;
        CorrectCount = 0;
        ErrorCount = 0;
        CurrentWPM = 0;
        CurrentCPM = 0;
        CurrentAccuracy = 100;
        ProgressValue = 0;
        IsGameActive = false;
        IsGameCompleted = false;
        IsGameNotStarted = true;
        ElapsedTime = "00:00";
    }

    /// <summary>
    /// Called when user types a character. This is the core game logic.
    /// </summary>
    [RelayCommand]
    private void ProcessInput(string input)
    {
        if (IsGameCompleted || string.IsNullOrEmpty(TargetText))
            return;

        // Start game on first input
        if (IsGameNotStarted)
        {
            StartGame();
        }

        TypedText = input;
        _session.TypedText = input;

        UpdateCharacterStates();
        UpdateStatistics();

        // Check if game is completed
        if (TypedText.Length >= TargetText.Length)
        {
            CompleteGame();
        }
    }

    private void StartGame()
    {
        IsGameNotStarted = false;
        IsGameActive = true;
        _session.StartTime = DateTime.Now;

        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += OnTimerElapsed;
        _timer.Start();
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var elapsed = DateTime.Now - _session.StartTime;
            ElapsedTime = $"{(int)elapsed.TotalMinutes:D2}:{elapsed.Seconds:D2}";
            UpdateStatistics();
        });
    }

    private void UpdateCharacterStates()
    {
        var states = new List<CharacterState>();

        for (int i = 0; i < TargetText.Length; i++)
        {
            var state = new CharacterState
            {
                Character = TargetText[i],
                Position = i
            };

            if (i < TypedText.Length)
            {
                state.State = TypedText[i] == TargetText[i]
                    ? CharState.Correct
                    : CharState.Error;
            }
            else if (i == TypedText.Length)
            {
                state.State = CharState.Current;
            }
            else
            {
                state.State = CharState.Pending;
            }

            states.Add(state);
        }

        CharacterStates = states;
        CurrentPosition = TypedText.Length;

        // Count correct and errors
        int correct = 0;
        int errors = 0;
        for (int i = 0; i < TypedText.Length && i < TargetText.Length; i++)
        {
            if (TypedText[i] == TargetText[i])
                correct++;
            else
                errors++;
        }

        CorrectCount = correct;
        ErrorCount = errors;
        _session.CorrectCharacters = correct;
        _session.ErrorCount = errors;

        ProgressValue = (double)TypedText.Length / TargetText.Length;
    }

    private void UpdateStatistics()
    {
        var elapsed = _session.ElapsedSeconds;
        if (elapsed > 0)
        {
            CurrentCPM = CorrectCount / elapsed * 60;
            CurrentWPM = CurrentCPM / 5.0;
        }

        if (TypedText.Length > 0)
        {
            CurrentAccuracy = (double)CorrectCount / TypedText.Length * 100;
        }
    }

    private async void CompleteGame()
    {
        _timer?.Stop();
        _timer?.Dispose();

        IsGameActive = false;
        IsGameCompleted = true;
        _session.EndTime = DateTime.Now;
        _session.IsCompleted = true;

        ResultWPM = Math.Round(_session.WPM, 1);
        ResultCPM = Math.Round(_session.CPM, 1);
        ResultAccuracy = Math.Round(_session.Accuracy, 1);
        ResultErrors = _session.ErrorCount;

        var elapsed = _session.EndTime!.Value - _session.StartTime;
        ResultTime = $"{(int)elapsed.TotalMinutes:D2}:{elapsed.Seconds:D2}";

        // Save result
        var result = new SessionResult
        {
            DictionaryId = _session.DictionaryId,
            DictionaryName = _session.DictionaryName,
            WPM = _session.WPM,
            CPM = _session.CPM,
            Accuracy = _session.Accuracy,
            TotalCharacters = _session.TotalCharacters,
            CorrectCharacters = _session.CorrectCharacters,
            ErrorCount = _session.ErrorCount,
            DurationSeconds = _session.ElapsedSeconds
        };

        await _statisticsService.AddResultAsync(result);
    }

    [RelayCommand]
    private async Task RestartGameAsync()
    {
        _timer?.Stop();
        _timer?.Dispose();
        await InitializeGameAsync();
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        _timer?.Stop();
        _timer?.Dispose();
        await Shell.Current.GoToAsync("..");
    }
}

/// <summary>
/// Represents the state of a single character in the target text.
/// </summary>
public class CharacterState
{
    public char Character { get; set; }
    public int Position { get; set; }
    public CharState State { get; set; } = CharState.Pending;
}

/// <summary>
/// Possible states for a character in the typing game.
/// </summary>
public enum CharState
{
    Pending,
    Current,
    Correct,
    Error
}
