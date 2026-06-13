using Newtonsoft.Json;

namespace CheckersGame.Models;

/// <summary>
/// Модель хода шашки.
/// Описывает перемещение шашки из одной клетки в другую,
/// включая информацию о захваченных шашках и цепочках взятий.
/// </summary>
public class MoveModel
{
    /// <summary>
    /// Строка начальной позиции шашки.
    /// </summary>
    [JsonProperty("fromRow")]
    public int FromRow { get; set; }

    /// <summary>
    /// Столбец начальной позиции шашки.
    /// </summary>
    [JsonProperty("fromCol")]
    public int FromCol { get; set; }

    /// <summary>
    /// Строка конечной позиции шашки.
    /// </summary>
    [JsonProperty("toRow")]
    public int ToRow { get; set; }

    /// <summary>
    /// Столбец конечной позиции шашки.
    /// </summary>
    [JsonProperty("toCol")]
    public int ToCol { get; set; }

    /// <summary>
    /// Список захваченных (побитых) шашек противника при выполнении этого хода.
    /// Пустой список, если ход без взятия.
    /// </summary>
    [JsonProperty("capturedPieces")]
    public List<CapturedPieceInfo> CapturedPieces { get; set; } = new();

    /// <summary>
    /// Флаг превращения шашки в дамку при достижении последнего ряда.
    /// </summary>
    [JsonProperty("isPromotion")]
    public bool IsPromotion { get; set; }

    /// <summary>
    /// Флаг цепочки взятий (серия последовательных прыжков через шашки противника).
    /// </summary>
    [JsonProperty("isChainCapture")]
    public bool IsChainCapture { get; set; }

    /// <summary>
    /// Цвет шашки, выполняющей ход.
    /// </summary>
    [JsonProperty("pieceColor")]
    public PieceColor PieceColor { get; set; }

    /// <summary>
    /// Тип шашки до выполнения хода (Man или King).
    /// Используется для корректного undo при превращении в дамку.
    /// </summary>
    [JsonProperty("pieceTypeBefore")]
    public PieceType PieceTypeBefore { get; set; }

    /// <summary>
    /// Детальные шаги в цепочке взятий.
    /// Каждый шаг содержит промежуточные координаты и захваченную шашку.
    /// Используется для корректного undo цепочки.
    /// </summary>
    [JsonProperty("chainSteps")]
    public List<ChainStep> ChainSteps { get; set; } = new();

    /// <summary>
    /// Преобразует ход в стандартную шашечную нотацию.
    /// Формат: "номер_клетки-номер_клетки" для простого хода,
    /// "номер_клетки:номер_клетки" для хода со взятием.
    /// </summary>
    /// <param name="boardSize">Размер доски (8 или 10).</param>
    /// <returns>Строка нотации хода.</returns>
    public string ToNotation(int boardSize)
    {
        string from = CellToNotation(FromRow, FromCol, boardSize);
        string to = CellToNotation(ToRow, ToCol, boardSize);
        // Разделитель: ":" для взятия, "-" для простого хода
        string separator = CapturedPieces.Count > 0 ? ":" : "-";
        return $"{from}{separator}{to}";
    }

    /// <summary>
    /// Преобразует координаты клетки в номер по стандартной нотации шашек.
    /// Нумеруются только тёмные (игровые) клетки, слева направо, сверху вниз.
    /// </summary>
    /// <param name="row">Строка клетки.</param>
    /// <param name="col">Столбец клетки.</param>
    /// <param name="boardSize">Размер доски.</param>
    /// <returns>Номер клетки в виде строки.</returns>
    private static string CellToNotation(int row, int col, int boardSize)
    {
        // Стандартная нотация шашек: нумерация тёмных клеток
        int number = 0;
        for (int r = 0; r < boardSize; r++)
        {
            for (int c = 0; c < boardSize; c++)
            {
                if ((r + c) % 2 == 1)
                {
                    number++;
                    if (r == row && c == col)
                        return number.ToString();
                }
            }
        }
        // Запасной вариант: алгебраическая нотация (a1, b2, ...)
        return $"{(char)('a' + col)}{boardSize - row}";
    }
}

/// <summary>
/// Информация о захваченной (побитой) шашке.
/// Хранит позицию и копию шашки для возможности отмены хода.
/// </summary>
public class CapturedPieceInfo
{
    /// <summary>
    /// Строка, на которой стояла захваченная шашка.
    /// </summary>
    [JsonProperty("row")]
    public int Row { get; set; }

    /// <summary>
    /// Столбец, на котором стояла захваченная шашка.
    /// </summary>
    [JsonProperty("col")]
    public int Col { get; set; }

    /// <summary>
    /// Копия захваченной шашки (для восстановления при undo).
    /// </summary>
    [JsonProperty("piece")]
    public PieceModel Piece { get; set; } = null!;
}

/// <summary>
/// Один шаг в цепочке взятий.
/// Описывает промежуточный прыжок: откуда, куда и какая шашка была захвачена.
/// </summary>
public class ChainStep
{
    /// <summary>
    /// Строка начальной позиции шага.
    /// </summary>
    [JsonProperty("fromRow")]
    public int FromRow { get; set; }

    /// <summary>
    /// Столбец начальной позиции шага.
    /// </summary>
    [JsonProperty("fromCol")]
    public int FromCol { get; set; }

    /// <summary>
    /// Строка конечной позиции шага.
    /// </summary>
    [JsonProperty("toRow")]
    public int ToRow { get; set; }

    /// <summary>
    /// Столбец конечной позиции шага.
    /// </summary>
    [JsonProperty("toCol")]
    public int ToCol { get; set; }

    /// <summary>
    /// Строка захваченной шашки (между начальной и конечной позицией).
    /// </summary>
    [JsonProperty("capturedRow")]
    public int CapturedRow { get; set; }

    /// <summary>
    /// Столбец захваченной шашки.
    /// </summary>
    [JsonProperty("capturedCol")]
    public int CapturedCol { get; set; }

    /// <summary>
    /// Копия захваченной шашки (для восстановления при undo).
    /// </summary>
    [JsonProperty("capturedPiece")]
    public PieceModel CapturedPiece { get; set; } = null!;
}
