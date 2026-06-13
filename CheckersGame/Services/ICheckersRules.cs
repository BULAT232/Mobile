using CheckersGame.Models;

namespace CheckersGame.Services;

/// <summary>
/// Интерфейс правил шашек.
/// Определяет контракт для всех вариантов шашечных правил (русские, международные и т.д.).
/// Каждая реализация задаёт размер доски, начальную расстановку, допустимые ходы,
/// правила взятия, превращения в дамку и условия окончания игры.
/// </summary>
public interface ICheckersRules
{
    /// <summary>
    /// Размер доски (8 для русских/бразильских/поддавков, 10 для международных).
    /// </summary>
    int BoardSize { get; }

    /// <summary>
    /// Тип игры, реализуемый данными правилами.
    /// </summary>
    GameType GameType { get; }

    /// <summary>
    /// Создаёт доску с начальной расстановкой шашек.
    /// Белые располагаются внизу, чёрные — вверху.
    /// </summary>
    /// <returns>Двумерный массив доски с расставленными шашками.</returns>
    PieceModel?[][] InitializeBoard();

    /// <summary>
    /// Получает все допустимые ходы для указанного цвета.
    /// Если есть обязательные взятия, возвращает только ходы со взятием.
    /// </summary>
    /// <param name="board">Текущее состояние доски.</param>
    /// <param name="color">Цвет игрока, для которого ищутся ходы.</param>
    /// <returns>Список допустимых ходов.</returns>
    List<MoveModel> GetValidMoves(PieceModel?[][] board, PieceColor color);

    /// <summary>
    /// Получает допустимые ходы для конкретной шашки на указанной позиции.
    /// Учитывает правило обязательного взятия.
    /// </summary>
    /// <param name="board">Текущее состояние доски.</param>
    /// <param name="row">Строка шашки.</param>
    /// <param name="col">Столбец шашки.</param>
    /// <returns>Список допустимых ходов для данной шашки.</returns>
    List<MoveModel> GetValidMovesForPiece(PieceModel?[][] board, int row, int col);

    /// <summary>
    /// Проверяет, есть ли обязательное взятие у указанного цвета.
    /// В шашках взятие обязательно, если оно возможно.
    /// </summary>
    /// <param name="board">Текущее состояние доски.</param>
    /// <param name="color">Цвет игрока.</param>
    /// <returns>True, если есть хотя бы одно возможное взятие.</returns>
    bool MustCapture(PieceModel?[][] board, PieceColor color);

    /// <summary>
    /// Проверяет, может ли шашка указанного цвета стать дамкой на данной строке.
    /// Белые становятся дамками на строке 0, чёрные — на последней строке.
    /// </summary>
    /// <param name="row">Строка, на которую перемещается шашка.</param>
    /// <param name="color">Цвет шашки.</param>
    /// <returns>True, если шашка должна стать дамкой.</returns>
    bool CanPromote(int row, PieceColor color);

    /// <summary>
    /// Проверяет, закончена ли игра для текущего игрока.
    /// Игра заканчивается, если у игрока нет допустимых ходов или шашек.
    /// </summary>
    /// <param name="board">Текущее состояние доски.</param>
    /// <param name="currentTurn">Цвет текущего игрока.</param>
    /// <returns>True, если игра окончена.</returns>
    bool IsGameOver(PieceModel?[][] board, PieceColor currentTurn);

    /// <summary>
    /// Определяет результат завершённой игры.
    /// Проигрывает тот, кто не может сделать ход (кроме поддавков).
    /// </summary>
    /// <param name="board">Текущее состояние доски.</param>
    /// <param name="currentTurn">Цвет текущего игрока.</param>
    /// <returns>Статус игры (победа белых/чёрных, ничья или продолжение).</returns>
    GameStatus GetGameResult(PieceModel?[][] board, PieceColor currentTurn);

    /// <summary>
    /// Имеет ли дамка «летающее» движение (может ходить на несколько клеток по диагонали).
    /// True для международных и бразильских шашек.
    /// </summary>
    bool HasFlyingKings { get; }
}
