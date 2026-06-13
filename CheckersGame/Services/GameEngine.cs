using CheckersGame.Models;

namespace CheckersGame.Services;

/// <summary>
/// Игровой движок шашек.
/// Управляет состоянием доски, выполнением ходов, отменой/повтором ходов (undo/redo),
/// таймерами и определением окончания игры.
/// Является центральным компонентом игровой логики.
/// </summary>
public class GameEngine
{
    /// <summary>
    /// Текущие правила игры (зависят от выбранного типа шашек).
    /// </summary>
    private ICheckersRules _rules = null!;

    /// <summary>
    /// Текущее состояние игры (доска, ходы, таймеры и т.д.).
    /// </summary>
    private GameState _state = null!;

    /// <summary>
    /// Стек сохранённых состояний для отмены ходов (undo).
    /// </summary>
    private readonly Stack<GameState> _undoStack = new();

    /// <summary>
    /// Стек сохранённых состояний для повтора отменённых ходов (redo).
    /// </summary>
    private readonly Stack<GameState> _redoStack = new();

    /// <summary>
    /// Текущее состояние игры (только для чтения).
    /// </summary>
    public GameState State => _state;

    /// <summary>
    /// Текущие правила игры (только для чтения).
    /// </summary>
    public ICheckersRules Rules => _rules;

    /// <summary>
    /// Фабричный метод: создаёт объект правил для указанного типа игры.
    /// </summary>
    /// <param name="gameType">Тип игры.</param>
    /// <returns>Реализация правил для данного типа.</returns>
    /// <exception cref="ArgumentException">Если тип игры неизвестен.</exception>
    public static ICheckersRules CreateRules(GameType gameType)
    {
        return gameType switch
        {
            GameType.Russian => new RussianCheckersRules(),
            GameType.International => new InternationalCheckersRules(),
            GameType.Brazilian => new BrazilianCheckersRules(),
            GameType.Giveaway => new GiveawayCheckersRules(),
            _ => throw new ArgumentException($"Неизвестный тип игры: {gameType}")
        };
    }

    /// <summary>
    /// Инициализирует новую игру: создаёт правила, расставляет шашки,
    /// настраивает таймеры и очищает историю undo/redo.
    /// </summary>
    /// <param name="gameType">Тип игры (русские, международные и т.д.).</param>
    /// <param name="player1Id">Идентификатор первого игрока (белые).</param>
    /// <param name="player2Id">Идентификатор второго игрока (чёрные).</param>
    /// <param name="enableTimer">Включить ли таймер.</param>
    /// <param name="timerMinutes">Количество минут на таймере для каждого игрока.</param>
    public void StartNewGame(GameType gameType, string player1Id, string player2Id,
        bool enableTimer = false, int timerMinutes = 10)
    {
        _rules = CreateRules(gameType);
        _state = new GameState
        {
            Board = _rules.InitializeBoard(),
            CurrentTurn = PieceColor.White,
            Status = GameStatus.InProgress,
            GameType = gameType,
            BoardSize = _rules.BoardSize,
            StartTime = DateTime.Now,
            Player1Id = player1Id,
            Player2Id = player2Id,
            WhiteTimeRemainingMs = enableTimer ? timerMinutes * 60 * 1000L : 0,
            BlackTimeRemainingMs = enableTimer ? timerMinutes * 60 * 1000L : 0
        };

        _undoStack.Clear();
        _redoStack.Clear();
    }

    /// <summary>
    /// Получает все допустимые ходы для текущего игрока.
    /// Если есть обязательные взятия, возвращает только ходы со взятием.
    /// </summary>
    /// <returns>Список допустимых ходов. Пустой список, если игра окончена.</returns>
    public List<MoveModel> GetCurrentValidMoves()
    {
        if (_state.Status != GameStatus.InProgress)
            return new List<MoveModel>();

        return _rules.GetValidMoves(_state.Board, _state.CurrentTurn);
    }

    /// <summary>
    /// Получает допустимые ходы для конкретной клетки.
    /// Фильтрует общие ходы текущего цвета, оставляя только ходы из указанной позиции.
    /// </summary>
    /// <param name="row">Строка клетки.</param>
    /// <param name="col">Столбец клетки.</param>
    /// <returns>Список допустимых ходов для шашки на данной клетке.</returns>
    public List<MoveModel> GetValidMovesForCell(int row, int col)
    {
        if (_state.Status != GameStatus.InProgress)
            return new List<MoveModel>();

        var piece = _state.Board[row][col];
        if (piece == null || piece.Color != _state.CurrentTurn)
            return new List<MoveModel>();

        // Получаем все ходы для текущего цвета (с учётом обязательного взятия)
        var allMoves = _rules.GetValidMoves(_state.Board, _state.CurrentTurn);

        // Фильтруем только ходы из данной клетки
        return allMoves.Where(m => m.FromRow == row && m.FromCol == col).ToList();
    }

    /// <summary>
    /// Выполняет ход на доске.
    /// Проверяет допустимость хода, перемещает шашку, удаляет захваченные шашки,
    /// проверяет превращение в дамку и переключает ход.
    /// Сохраняет состояние для undo и очищает стек redo.
    /// </summary>
    /// <param name="move">Ход для выполнения.</param>
    /// <returns>True, если ход выполнен успешно; False, если ход недопустим.</returns>
    public bool ExecuteMove(MoveModel move)
    {
        if (_state.Status != GameStatus.InProgress)
            return false;

        // Проверяем, что ход допустим
        var validMoves = GetCurrentValidMoves();
        var matchingMove = validMoves.FirstOrDefault(m =>
            m.FromRow == move.FromRow && m.FromCol == move.FromCol &&
            m.ToRow == move.ToRow && m.ToCol == move.ToCol);

        if (matchingMove == null)
            return false;

        // Сохраняем состояние для undo
        SaveStateForUndo();
        _redoStack.Clear();

        // Выполняем перемещение шашки
        var piece = _state.Board[move.FromRow][move.FromCol]!;
        _state.Board[move.FromRow][move.FromCol] = null;
        _state.Board[move.ToRow][move.ToCol] = piece;

        // Удаляем захваченные шашки с доски и обновляем счётчик
        foreach (var captured in matchingMove.CapturedPieces)
        {
            _state.Board[captured.Row][captured.Col] = null;

            if (_state.CurrentTurn == PieceColor.White)
                _state.WhiteCaptured++;
            else
                _state.BlackCaptured++;
        }

        // Превращение в дамку при достижении последнего ряда
        if (matchingMove.IsPromotion || _rules.CanPromote(move.ToRow, piece.Color))
        {
            piece.Type = PieceType.King;
            matchingMove.IsPromotion = true;
        }

        // Записываем ход в историю
        _state.MoveHistory.Add(matchingMove);

        // Переключаем ход на другого игрока
        _state.CurrentTurn = _state.CurrentTurn == PieceColor.White
            ? PieceColor.Black
            : PieceColor.White;

        // Проверяем окончание игры
        if (_rules.IsGameOver(_state.Board, _state.CurrentTurn))
        {
            _state.Status = _rules.GetGameResult(_state.Board, _state.CurrentTurn);
        }

        return true;
    }

    /// <summary>
    /// Выполняет ход по координатам (удобная обёртка).
    /// </summary>
    /// <param name="fromRow">Строка начальной позиции.</param>
    /// <param name="fromCol">Столбец начальной позиции.</param>
    /// <param name="toRow">Строка конечной позиции.</param>
    /// <param name="toCol">Столбец конечной позиции.</param>
    /// <returns>True, если ход выполнен успешно.</returns>
    public bool ExecuteMove(int fromRow, int fromCol, int toRow, int toCol)
    {
        var move = new MoveModel
        {
            FromRow = fromRow,
            FromCol = fromCol,
            ToRow = toRow,
            ToCol = toCol
        };
        return ExecuteMove(move);
    }

    /// <summary>
    /// Отменяет последний ход (undo).
    /// Восстанавливает предыдущее состояние доски из стека undo.
    /// Текущее состояние сохраняется в стек redo.
    /// </summary>
    /// <returns>True, если отмена выполнена; False, если нечего отменять.</returns>
    public bool Undo()
    {
        if (_undoStack.Count == 0)
            return false;

        // Сохраняем текущее состояние для redo
        _redoStack.Push(CloneState(_state));

        // Восстанавливаем предыдущее состояние
        _state = _undoStack.Pop();
        return true;
    }

    /// <summary>
    /// Повторяет отменённый ход (redo).
    /// Восстанавливает состояние из стека redo.
    /// Текущее состояние сохраняется в стек undo.
    /// </summary>
    /// <returns>True, если повтор выполнен; False, если нечего повторять.</returns>
    public bool Redo()
    {
        if (_redoStack.Count == 0)
            return false;

        _undoStack.Push(CloneState(_state));
        _state = _redoStack.Pop();
        return true;
    }

    /// <summary>
    /// Можно ли отменить ход (есть ли сохранённые состояния в стеке undo).
    /// </summary>
    public bool CanUndo => _undoStack.Count > 0;

    /// <summary>
    /// Можно ли повторить ход (есть ли сохранённые состояния в стеке redo).
    /// </summary>
    public bool CanRedo => _redoStack.Count > 0;

    /// <summary>
    /// Обновляет оставшееся время текущего игрока.
    /// Если время истекло, игра завершается поражением текущего игрока.
    /// </summary>
    /// <param name="elapsedMs">Прошедшее время в миллисекундах с последнего обновления.</param>
    public void UpdateTimer(long elapsedMs)
    {
        if (_state.Status != GameStatus.InProgress) return;

        if (_state.CurrentTurn == PieceColor.White)
        {
            _state.WhiteTimeRemainingMs -= elapsedMs;
            if (_state.WhiteTimeRemainingMs <= 0)
            {
                _state.WhiteTimeRemainingMs = 0;
                _state.Status = GameStatus.BlackWins;
            }
        }
        else
        {
            _state.BlackTimeRemainingMs -= elapsedMs;
            if (_state.BlackTimeRemainingMs <= 0)
            {
                _state.BlackTimeRemainingMs = 0;
                _state.Status = GameStatus.WhiteWins;
            }
        }
    }

    /// <summary>
    /// Завершает игру ничьей (по соглашению сторон).
    /// </summary>
    public void DeclareDraw()
    {
        _state.Status = GameStatus.Draw;
    }

    /// <summary>
    /// Текущий игрок сдаётся. Противник объявляется победителем.
    /// </summary>
    public void Resign()
    {
        _state.Status = _state.CurrentTurn == PieceColor.White
            ? GameStatus.BlackWins
            : GameStatus.WhiteWins;
    }

    /// <summary>
    /// Получает длительность текущей игры от момента старта до текущего момента.
    /// </summary>
    /// <returns>Длительность игры.</returns>
    public TimeSpan GetGameDuration()
    {
        return DateTime.Now - _state.StartTime;
    }

    /// <summary>
    /// Подсчитывает количество белых и чёрных шашек на доске.
    /// </summary>
    /// <returns>Кортеж (количество белых, количество чёрных).</returns>
    public (int white, int black) CountPieces()
    {
        int white = 0, black = 0;
        for (int r = 0; r < _state.BoardSize; r++)
            for (int c = 0; c < _state.BoardSize; c++)
            {
                if (_state.Board[r][c]?.Color == PieceColor.White) white++;
                if (_state.Board[r][c]?.Color == PieceColor.Black) black++;
            }
        return (white, black);
    }

    /// <summary>
    /// Сохраняет текущее состояние в стек undo (перед выполнением хода).
    /// </summary>
    private void SaveStateForUndo()
    {
        _undoStack.Push(CloneState(_state));
    }

    /// <summary>
    /// Создаёт глубокую копию состояния игры.
    /// Клонирует доску и историю ходов для независимого хранения.
    /// </summary>
    /// <param name="state">Состояние для клонирования.</param>
    /// <returns>Новый объект GameState — полная копия.</returns>
    private GameState CloneState(GameState state)
    {
        return new GameState
        {
            Board = state.CloneBoard(),
            CurrentTurn = state.CurrentTurn,
            MoveHistory = new List<MoveModel>(state.MoveHistory),
            Status = state.Status,
            GameType = state.GameType,
            BoardSize = state.BoardSize,
            WhiteTimeRemainingMs = state.WhiteTimeRemainingMs,
            BlackTimeRemainingMs = state.BlackTimeRemainingMs,
            StartTime = state.StartTime,
            Player1Id = state.Player1Id,
            Player2Id = state.Player2Id,
            WhiteCaptured = state.WhiteCaptured,
            BlackCaptured = state.BlackCaptured
        };
    }
}
