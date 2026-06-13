using Newtonsoft.Json;

namespace CheckersGame.Models;

/// <summary>
/// Полное состояние текущей игры.
/// Содержит доску, историю ходов, таймеры и статус партии.
/// Используется для сохранения/восстановления состояния (undo/redo).
/// </summary>
public class GameState
{
    /// <summary>
    /// Двумерный массив доски: Board[row][col].
    /// Null означает пустую клетку, иначе — шашку на данной позиции.
    /// </summary>
    [JsonProperty("board")]
    public PieceModel?[][] Board { get; set; } = null!;

    /// <summary>
    /// Цвет игрока, чей сейчас ход.
    /// </summary>
    [JsonProperty("currentTurn")]
    public PieceColor CurrentTurn { get; set; } = PieceColor.White;

    /// <summary>
    /// Хронологический список всех выполненных ходов в партии.
    /// </summary>
    [JsonProperty("moveHistory")]
    public List<MoveModel> MoveHistory { get; set; } = new();

    /// <summary>
    /// Текущий статус игры (не начата, в процессе, победа, ничья).
    /// </summary>
    [JsonProperty("status")]
    public GameStatus Status { get; set; } = GameStatus.NotStarted;

    /// <summary>
    /// Тип игры (русские, международные, бразильские, поддавки).
    /// </summary>
    [JsonProperty("gameType")]
    public GameType GameType { get; set; }

    /// <summary>
    /// Размер доски (8 для русских/бразильских/поддавков, 10 для международных).
    /// </summary>
    [JsonProperty("boardSize")]
    public int BoardSize { get; set; } = 8;

    /// <summary>
    /// Оставшееся время белых в миллисекундах. 0, если таймер не используется.
    /// </summary>
    [JsonProperty("whiteTimeRemainingMs")]
    public long WhiteTimeRemainingMs { get; set; }

    /// <summary>
    /// Оставшееся время чёрных в миллисекундах. 0, если таймер не используется.
    /// </summary>
    [JsonProperty("blackTimeRemainingMs")]
    public long BlackTimeRemainingMs { get; set; }

    /// <summary>
    /// Время начала партии.
    /// </summary>
    [JsonProperty("startTime")]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Идентификатор первого игрока (белые).
    /// </summary>
    [JsonProperty("player1Id")]
    public string Player1Id { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор второго игрока (чёрные).
    /// </summary>
    [JsonProperty("player2Id")]
    public string Player2Id { get; set; } = string.Empty;

    /// <summary>
    /// Количество шашек, захваченных белыми за партию.
    /// </summary>
    [JsonProperty("whiteCaptured")]
    public int WhiteCaptured { get; set; }

    /// <summary>
    /// Количество шашек, захваченных чёрными за партию.
    /// </summary>
    [JsonProperty("blackCaptured")]
    public int BlackCaptured { get; set; }

    /// <summary>
    /// Создаёт глубокую копию доски (все шашки клонируются).
    /// Используется для сохранения состояния при undo/redo.
    /// </summary>
    /// <returns>Новый двумерный массив с клонированными шашками.</returns>
    public PieceModel?[][] CloneBoard()
    {
        var newBoard = new PieceModel?[BoardSize][];
        for (int r = 0; r < BoardSize; r++)
        {
            newBoard[r] = new PieceModel?[BoardSize];
            for (int c = 0; c < BoardSize; c++)
            {
                newBoard[r][c] = Board[r][c]?.Clone();
            }
        }
        return newBoard;
    }
}
