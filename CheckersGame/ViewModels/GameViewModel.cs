using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CheckersGame.Models;
using CheckersGame.Services;

namespace CheckersGame.ViewModels;

/// <summary>
/// ViewModel игровой страницы.
/// </summary>
[QueryProperty(nameof(GameTypeParam), "GameType")]
[QueryProperty(nameof(Player1Param), "Player1")]
[QueryProperty(nameof(Player2Param), "Player2")]
[QueryProperty(nameof(EnableTimerParam), "EnableTimer")]
[QueryProperty(nameof(TimerMinutesParam), "TimerMinutes")]
public partial class GameViewModel : ObservableObject
{
    private readonly ProfileService _profileService;
    private readonly StatisticsService _statisticsService;
    private readonly SettingsService _settingsService;
    private readonly GameEngine _engine = new();
    private IDispatcherTimer? _timer;
    private DateTime _lastTimerTick;
    private bool _showHints = true;

    // Query parameters
    public GameType GameTypeParam { get; set; }
    public PlayerProfile Player1Param { get; set; } = null!;
    public PlayerProfile Player2Param { get; set; } = null!;
    public bool EnableTimerParam { get; set; }
    public int TimerMinutesParam { get; set; }

    [ObservableProperty]
    private ObservableCollection<CellModel> _cells = new();

    [ObservableProperty]
    private ObservableCollection<string> _moveLog = new();

    [ObservableProperty]
    private string _currentTurnText = string.Empty;

    [ObservableProperty]
    private string _player1Name = string.Empty;

    [ObservableProperty]
    private string _player2Name = string.Empty;

    [ObservableProperty]
    private string _player1Timer = string.Empty;

    [ObservableProperty]
    private string _player2Timer = string.Empty;

    [ObservableProperty]
    private string _gameStatusText = string.Empty;

    [ObservableProperty]
    private bool _isGameOver;

    [ObservableProperty]
    private bool _isTimerEnabled;

    [ObservableProperty]
    private bool _canUndo;

    [ObservableProperty]
    private bool _canRedo;

    [ObservableProperty]
    private int _boardSize = 8;

    [ObservableProperty]
    private string _whitePiecesCount = "12";

    [ObservableProperty]
    private string _blackPiecesCount = "12";

    [ObservableProperty]
    private string _gameTypeDisplay = string.Empty;

    [ObservableProperty]
    private bool _isPlayer1Turn = true;

    private CellModel? _selectedCell;
    private List<MoveModel> _currentValidMoves = new();

    public GameViewModel(ProfileService profileService, StatisticsService statisticsService,
        SettingsService settingsService)
    {
        _profileService = profileService;
        _statisticsService = statisticsService;
        _settingsService = settingsService;
    }

    [RelayCommand]
    private async Task InitializeGameAsync()
    {
        var settings = await _settingsService.GetSettingsAsync();
        _showHints = settings.ShowHints;

        _engine.StartNewGame(GameTypeParam, Player1Param.Id, Player2Param.Id,
            EnableTimerParam, TimerMinutesParam);

        BoardSize = _engine.Rules.BoardSize;
        Player1Name = $"⛀ {Player1Param.Name}";
        Player2Name = $"⛂ {Player2Param.Name}";
        IsTimerEnabled = EnableTimerParam;

        GameTypeDisplay = GameTypeParam switch
        {
            GameType.Russian => "Русские шашки",
            GameType.International => "Международные шашки",
            GameType.Brazilian => "Бразильские шашки",
            GameType.Giveaway => "Поддавки",
            _ => ""
        };

        RefreshBoard();
        UpdateStatus();

        if (EnableTimerParam)
        {
            StartTimer();
        }
    }

    [RelayCommand]
    private void CellTapped(CellModel cell)
    {
        if (IsGameOver || _engine.State.Status != GameStatus.InProgress)
            return;

        if (_selectedCell != null && cell.IsValidTarget)
        {
            // Выполняем ход
            var move = _currentValidMoves.FirstOrDefault(m =>
                m.FromRow == _selectedCell.Row && m.FromCol == _selectedCell.Col &&
                m.ToRow == cell.Row && m.ToCol == cell.Col);

            if (move != null)
            {
                ExecuteMove(move);
            }
            return;
        }

        // Выбираем шашку
        if (cell.Piece != null && cell.Piece.Color == _engine.State.CurrentTurn)
        {
            ClearHighlights();

            _selectedCell = cell;
            cell.IsSelected = true;

            // Показываем допустимые ходы
            _currentValidMoves = _engine.GetValidMovesForCell(cell.Row, cell.Col);

            if (_showHints)
            {
                foreach (var move in _currentValidMoves)
                {
                    var targetCell = GetCell(move.ToRow, move.ToCol);
                    if (targetCell != null)
                        targetCell.IsValidTarget = true;
                }
            }
        }
        else
        {
            ClearHighlights();
            _selectedCell = null;
            _currentValidMoves.Clear();
        }
    }

    private void ExecuteMove(MoveModel move)
    {
        bool success = _engine.ExecuteMove(move);
        if (!success) return;

        ClearHighlights();
        _selectedCell = null;
        _currentValidMoves.Clear();

        // Добавляем в лог
        string notation = move.ToNotation(BoardSize);
        string prefix = move.PieceColor == PieceColor.White ? "Б" : "Ч";
        MoveLog.Add($"{MoveLog.Count + 1}. {prefix}: {notation}");

        RefreshBoard();
        UpdateStatus();

        CanUndo = _engine.CanUndo;
        CanRedo = _engine.CanRedo;

        if (_engine.State.Status != GameStatus.InProgress)
        {
            OnGameOver();
        }
    }

    [RelayCommand]
    private void Undo()
    {
        if (_engine.Undo())
        {
            ClearHighlights();
            _selectedCell = null;
            _currentValidMoves.Clear();

            if (MoveLog.Count > 0)
                MoveLog.RemoveAt(MoveLog.Count - 1);

            RefreshBoard();
            UpdateStatus();

            CanUndo = _engine.CanUndo;
            CanRedo = _engine.CanRedo;
            IsGameOver = false;
        }
    }

    [RelayCommand]
    private void Redo()
    {
        if (_engine.Redo())
        {
            ClearHighlights();
            _selectedCell = null;
            _currentValidMoves.Clear();

            // Восстанавливаем лог
            var lastMove = _engine.State.MoveHistory.LastOrDefault();
            if (lastMove != null)
            {
                string notation = lastMove.ToNotation(BoardSize);
                string prefix = lastMove.PieceColor == PieceColor.White ? "Б" : "Ч";
                MoveLog.Add($"{MoveLog.Count + 1}. {prefix}: {notation}");
            }

            RefreshBoard();
            UpdateStatus();

            CanUndo = _engine.CanUndo;
            CanRedo = _engine.CanRedo;

            if (_engine.State.Status != GameStatus.InProgress)
                OnGameOver();
        }
    }

    [RelayCommand]
    private async Task ResignAsync()
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "Сдаться", "Вы уверены, что хотите сдаться?", "Да", "Нет");

        if (confirm)
        {
            _engine.Resign();
            UpdateStatus();
            OnGameOver();
        }
    }

    [RelayCommand]
    private async Task OfferDrawAsync()
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "Ничья", "Предложить ничью?", "Да", "Нет");

        if (confirm)
        {
            bool accept = await Shell.Current.DisplayAlert(
                "Ничья",
                $"{(_engine.State.CurrentTurn == PieceColor.White ? Player2Param.Name : Player1Param.Name)}, принять ничью?",
                "Принять", "Отклонить");

            if (accept)
            {
                _engine.DeclareDraw();
                UpdateStatus();
                OnGameOver();
            }
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        if (_engine.State.Status == GameStatus.InProgress)
        {
            bool confirm = await Shell.Current.DisplayAlert(
                "Выход", "Игра не завершена. Выйти?", "Да", "Нет");
            if (!confirm) return;
        }

        StopTimer();
        await Shell.Current.GoToAsync("..");
    }

    private void RefreshBoard()
    {
        var state = _engine.State;
        var newCells = new ObservableCollection<CellModel>();

        for (int r = 0; r < state.BoardSize; r++)
        {
            for (int c = 0; c < state.BoardSize; c++)
            {
                bool isPlayable = (r + c) % 2 == 1;
                var cell = new CellModel(r, c, isPlayable)
                {
                    Piece = state.Board[r][c]
                };
                newCells.Add(cell);
            }
        }

        // Assign the new collection to trigger PropertyChanged and re-render
        Cells = newCells;

        // Подсветка последнего хода
        if (state.MoveHistory.Count > 0)
        {
            var lastMove = state.MoveHistory.Last();
            var fromCell = GetCell(lastMove.FromRow, lastMove.FromCol);
            var toCell = GetCell(lastMove.ToRow, lastMove.ToCol);
            if (fromCell != null) fromCell.IsLastMove = true;
            if (toCell != null) toCell.IsLastMove = true;
        }

        var (white, black) = _engine.CountPieces();
        WhitePiecesCount = white.ToString();
        BlackPiecesCount = black.ToString();
    }

    private void UpdateStatus()
    {
        var state = _engine.State;

        IsPlayer1Turn = state.CurrentTurn == PieceColor.White;

        CurrentTurnText = state.Status switch
        {
            GameStatus.InProgress => state.CurrentTurn == PieceColor.White
                ? $"Ход: {Player1Param.Name} (белые)"
                : $"Ход: {Player2Param.Name} (чёрные)",
            GameStatus.WhiteWins => $"Победа: {Player1Param.Name}!",
            GameStatus.BlackWins => $"Победа: {Player2Param.Name}!",
            GameStatus.Draw => "Ничья!",
            _ => ""
        };

        if (IsTimerEnabled)
        {
            Player1Timer = FormatTime(state.WhiteTimeRemainingMs);
            Player2Timer = FormatTime(state.BlackTimeRemainingMs);
        }
    }

    private async void OnGameOver()
    {
        IsGameOver = true;
        StopTimer();

        var state = _engine.State;
        var duration = _engine.GetGameDuration();

        // Определяем победителя
        string? winnerId = state.Status switch
        {
            GameStatus.WhiteWins => Player1Param.Id,
            GameStatus.BlackWins => Player2Param.Id,
            _ => null
        };

        // Сохраняем запись об игре
        var record = new GameRecord
        {
            Player1Id = Player1Param.Id,
            Player1Name = Player1Param.Name,
            Player2Id = Player2Param.Id,
            Player2Name = Player2Param.Name,
            GameType = GameTypeParam,
            WinnerId = winnerId,
            Status = state.Status,
            MoveCount = state.MoveHistory.Count,
            Duration = duration,
            Player1Captured = state.WhiteCaptured,
            Player2Captured = state.BlackCaptured
        };

        await _statisticsService.AddRecordAsync(record);

        // Обновляем рейтинги
        if (state.Status == GameStatus.Draw)
        {
            await _profileService.UpdateRatingsAsync(Player1Param.Id, Player2Param.Id, isDraw: true);
        }
        else if (winnerId != null)
        {
            string loserId = winnerId == Player1Param.Id ? Player2Param.Id : Player1Param.Id;
            await _profileService.UpdateRatingsAsync(winnerId, loserId);
        }

        // Обновляем захваченные шашки
        await _profileService.UpdateCapturedCountAsync(Player1Param.Id, state.WhiteCaptured);
        await _profileService.UpdateCapturedCountAsync(Player2Param.Id, state.BlackCaptured);

        GameStatusText = CurrentTurnText;
    }

    private void ClearHighlights()
    {
        foreach (var cell in Cells)
        {
            cell.IsSelected = false;
            cell.IsValidTarget = false;
            cell.IsHighlighted = false;
        }
    }

    private CellModel? GetCell(int row, int col)
    {
        int index = row * BoardSize + col;
        if (index >= 0 && index < Cells.Count)
            return Cells[index];
        return null;
    }

    private void StartTimer()
    {
        _lastTimerTick = DateTime.Now;
        _timer = Application.Current?.Dispatcher.CreateTimer();
        if (_timer != null)
        {
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }
    }

    private void StopTimer()
    {
        _timer?.Stop();
        if (_timer != null)
            _timer.Tick -= OnTimerTick;
        _timer = null;
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        var elapsed = (long)(now - _lastTimerTick).TotalMilliseconds;
        _lastTimerTick = now;

        _engine.UpdateTimer(elapsed);

        var state = _engine.State;
        Player1Timer = FormatTime(state.WhiteTimeRemainingMs);
        Player2Timer = FormatTime(state.BlackTimeRemainingMs);

        if (state.Status != GameStatus.InProgress)
        {
            UpdateStatus();
            OnGameOver();
        }
    }

    private static string FormatTime(long milliseconds)
    {
        if (milliseconds <= 0) return "00:00";
        var ts = TimeSpan.FromMilliseconds(milliseconds);
        return ts.TotalHours >= 1
            ? ts.ToString(@"h\:mm\:ss")
            : ts.ToString(@"mm\:ss");
    }
}
