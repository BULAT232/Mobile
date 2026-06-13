using CheckersGame.Models;

namespace CheckersGame.Services;

/// <summary>
/// Русские шашки (8×8).
/// Особенности правил:
/// - Доска 8×8, по 12 шашек у каждого игрока.
/// - Простые шашки ходят вперёд по диагонали на 1 клетку.
/// - Простые шашки бьют во все 4 направления (вперёд и назад).
/// - Дамка ходит на любое количество клеток по диагонали («летающая» дамка).
/// - Дамка бьёт через шашку противника на любую свободную клетку за ней.
/// - Взятие обязательно. При нескольких вариантах выбирается максимальная цепочка.
/// - Шашка становится дамкой при достижении последнего ряда.
/// </summary>
public class RussianCheckersRules : ICheckersRules
{
    /// <summary>Размер доски — 8×8.</summary>
    public int BoardSize => 8;

    /// <summary>Тип игры — русские шашки.</summary>
    public GameType GameType => GameType.Russian;

    /// <summary>Дамки не «летающие» в классическом смысле (но ходят на любое расстояние).</summary>
    public bool HasFlyingKings => false;

    /// <summary>
    /// Создаёт доску 8×8 с начальной расстановкой:
    /// чёрные — строки 0–2, белые — строки 5–7, только на тёмных клетках.
    /// </summary>
    public PieceModel?[][] InitializeBoard()
    {
        var board = new PieceModel?[BoardSize][];
        for (int r = 0; r < BoardSize; r++)
        {
            board[r] = new PieceModel?[BoardSize];
            for (int c = 0; c < BoardSize; c++)
            {
                if ((r + c) % 2 == 1) // Только тёмные клетки
                {
                    if (r < 3)
                        board[r][c] = new PieceModel(PieceColor.Black);
                    else if (r > 4)
                        board[r][c] = new PieceModel(PieceColor.White);
                }
            }
        }
        return board;
    }

    /// <summary>
    /// Получает все допустимые ходы для указанного цвета.
    /// Если есть хотя бы одно взятие, возвращает только ходы со взятием (обязательное взятие).
    /// </summary>
    public List<MoveModel> GetValidMoves(PieceModel?[][] board, PieceColor color)
    {
        var captures = new List<MoveModel>();
        var simpleMoves = new List<MoveModel>();

        for (int r = 0; r < BoardSize; r++)
        {
            for (int c = 0; c < BoardSize; c++)
            {
                var piece = board[r][c];
                if (piece != null && piece.Color == color)
                {
                    var pieceMoves = GetValidMovesForPiece(board, r, c);
                    foreach (var move in pieceMoves)
                    {
                        if (move.CapturedPieces.Count > 0)
                            captures.Add(move);
                        else
                            simpleMoves.Add(move);
                    }
                }
            }
        }

        // Обязательное взятие: если есть взятия, простые ходы запрещены
        return captures.Count > 0 ? captures : simpleMoves;
    }

    /// <summary>
    /// Получает допустимые ходы для конкретной шашки.
    /// Учитывает правило обязательного взятия: если у любой шашки данного цвета
    /// есть взятие, то простые ходы запрещены для всех шашек.
    /// </summary>
    public List<MoveModel> GetValidMovesForPiece(PieceModel?[][] board, int row, int col)
    {
        var piece = board[row][col];
        if (piece == null) return new List<MoveModel>();

        // Сначала ищем взятия для данной шашки
        var captures = piece.Type == PieceType.King
            ? GetKingCaptures(board, row, col, piece)
            : GetManCaptures(board, row, col, piece);

        if (captures.Count > 0)
        {
            // Проверяем, есть ли вообще взятия у этого цвета
            bool anyCaptures = HasAnyCapturesForColor(board, piece.Color);
            if (anyCaptures)
                return captures;
        }

        // Если есть взятия у других шашек этого цвета, простые ходы запрещены
        if (HasAnyCapturesForColor(board, piece.Color))
            return captures; // Пустой список, если у этой шашки нет взятий

        // Нет взятий ни у кого — возвращаем простые ходы
        var simpleMoves = piece.Type == PieceType.King
            ? GetKingSimpleMoves(board, row, col, piece)
            : GetManSimpleMoves(board, row, col, piece);

        return simpleMoves;
    }

    /// <summary>
    /// Проверяет, есть ли хотя бы одно возможное взятие у любой шашки указанного цвета.
    /// </summary>
    /// <param name="board">Текущее состояние доски.</param>
    /// <param name="color">Цвет шашек для проверки.</param>
    /// <returns>True, если есть хотя бы одно взятие.</returns>
    private bool HasAnyCapturesForColor(PieceModel?[][] board, PieceColor color)
    {
        for (int r = 0; r < BoardSize; r++)
        {
            for (int c = 0; c < BoardSize; c++)
            {
                var p = board[r][c];
                if (p != null && p.Color == color)
                {
                    var caps = p.Type == PieceType.King
                        ? GetKingCaptures(board, r, c, p)
                        : GetManCaptures(board, r, c, p);
                    if (caps.Count > 0) return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Получает простые ходы (без взятия) для обычной шашки.
    /// Простая шашка ходит только вперёд по диагонали на 1 клетку.
    /// Белые ходят вверх (row уменьшается), чёрные — вниз (row увеличивается).
    /// </summary>
    private List<MoveModel> GetManSimpleMoves(PieceModel?[][] board, int row, int col, PieceModel piece)
    {
        var moves = new List<MoveModel>();
        // Направление движения: белые вверх (-1), чёрные вниз (+1)
        int direction = piece.Color == PieceColor.White ? -1 : 1;

        int[][] offsets = { new[] { direction, -1 }, new[] { direction, 1 } };

        foreach (var offset in offsets)
        {
            int newRow = row + offset[0];
            int newCol = col + offset[1];

            if (IsInBounds(newRow, newCol) && board[newRow][newCol] == null)
            {
                var move = new MoveModel
                {
                    FromRow = row,
                    FromCol = col,
                    ToRow = newRow,
                    ToCol = newCol,
                    PieceColor = piece.Color,
                    PieceTypeBefore = piece.Type,
                    IsPromotion = CanPromote(newRow, piece.Color)
                };
                moves.Add(move);
            }
        }

        return moves;
    }

    /// <summary>
    /// Получает все возможные взятия для обычной шашки.
    /// Шашка бьёт во все 4 направления, перепрыгивая через шашку противника.
    /// Поддерживает цепочки взятий (серийные прыжки).
    /// Выбирает максимальные цепочки (правило большинства).
    /// </summary>
    private List<MoveModel> GetManCaptures(PieceModel?[][] board, int row, int col, PieceModel piece)
    {
        var allCaptures = new List<MoveModel>();
        var visited = new HashSet<(int, int)>();
        FindManCaptureChains(board, row, col, piece, row, col, new List<ChainStep>(), new List<CapturedPieceInfo>(), visited, allCaptures);

        // Выбираем максимальные цепочки взятий (правило большинства)
        if (allCaptures.Count > 0)
        {
            int maxCaptures = allCaptures.Max(m => m.CapturedPieces.Count);
            allCaptures = allCaptures.Where(m => m.CapturedPieces.Count == maxCaptures).ToList();
        }

        return allCaptures;
    }

    /// <summary>
    /// Рекурсивно ищет все возможные цепочки взятий для обычной шашки.
    /// Временно модифицирует доску для проверки продолжения цепочки,
    /// затем восстанавливает исходное состояние (backtracking).
    /// </summary>
    /// <param name="board">Доска (модифицируется временно).</param>
    /// <param name="startRow">Начальная строка шашки (до первого прыжка).</param>
    /// <param name="startCol">Начальный столбец шашки.</param>
    /// <param name="piece">Шашка, выполняющая взятие.</param>
    /// <param name="currentRow">Текущая строка шашки в цепочке.</param>
    /// <param name="currentCol">Текущий столбец шашки в цепочке.</param>
    /// <param name="currentChain">Накопленные шаги цепочки.</param>
    /// <param name="capturedSoFar">Уже захваченные шашки в цепочке.</param>
    /// <param name="visited">Множество позиций уже захваченных шашек (нельзя бить дважды).</param>
    /// <param name="allCaptures">Результирующий список всех найденных цепочек.</param>
    private void FindManCaptureChains(PieceModel?[][] board, int startRow, int startCol,
        PieceModel piece, int currentRow, int currentCol,
        List<ChainStep> currentChain, List<CapturedPieceInfo> capturedSoFar,
        HashSet<(int, int)> visited, List<MoveModel> allCaptures)
    {
        // Шашки в русских шашках бьют во все 4 направления
        int[][] directions = { new[] { -1, -1 }, new[] { -1, 1 }, new[] { 1, -1 }, new[] { 1, 1 } };
        bool foundCapture = false;

        foreach (var dir in directions)
        {
            int midRow = currentRow + dir[0];
            int midCol = currentCol + dir[1];
            int landRow = currentRow + 2 * dir[0];
            int landCol = currentCol + 2 * dir[1];

            if (!IsInBounds(landRow, landCol)) continue;
            if (visited.Contains((midRow, midCol))) continue;

            var midPiece = board[midRow][midCol];
            if (midPiece == null || midPiece.Color == piece.Color) continue;
            if (board[landRow][landCol] != null && !(landRow == startRow && landCol == startCol)) continue;
            // Разрешаем приземление на стартовую позицию (шашка уже ушла оттуда)
            if (board[landRow][landCol] != null && landRow != startRow && landCol != startCol) continue;

            foundCapture = true;

            var step = new ChainStep
            {
                FromRow = currentRow,
                FromCol = currentCol,
                ToRow = landRow,
                ToCol = landCol,
                CapturedRow = midRow,
                CapturedCol = midCol,
                CapturedPiece = midPiece.Clone()
            };

            var capturedInfo = new CapturedPieceInfo
            {
                Row = midRow,
                Col = midCol,
                Piece = midPiece.Clone()
            };

            var newChain = new List<ChainStep>(currentChain) { step };
            var newCaptured = new List<CapturedPieceInfo>(capturedSoFar) { capturedInfo };
            var newVisited = new HashSet<(int, int)>(visited) { (midRow, midCol) };

            // Временно убираем захваченную шашку для продолжения цепочки
            var savedPiece = board[midRow][midCol];
            var savedLand = board[landRow][landCol];
            board[midRow][midCol] = null;
            board[landRow][landCol] = piece;
            board[currentRow][currentCol] = null;

            // Рекурсивно ищем продолжение цепочки
            FindManCaptureChains(board, startRow, startCol, piece, landRow, landCol,
                newChain, newCaptured, newVisited, allCaptures);

            // Восстанавливаем доску (backtracking)
            board[midRow][midCol] = savedPiece;
            board[landRow][landCol] = savedLand;
            board[currentRow][currentCol] = (currentRow == startRow && currentCol == startCol) ? piece : null;
            if (currentRow != startRow || currentCol != startCol)
                board[currentRow][currentCol] = piece;
        }

        // Если продолжения нет и цепочка не пуста — сохраняем результат
        if (!foundCapture && currentChain.Count > 0)
        {
            var move = new MoveModel
            {
                FromRow = startRow,
                FromCol = startCol,
                ToRow = currentRow,
                ToCol = currentCol,
                PieceColor = piece.Color,
                PieceTypeBefore = piece.Type,
                CapturedPieces = new List<CapturedPieceInfo>(capturedSoFar),
                ChainSteps = new List<ChainStep>(currentChain),
                IsChainCapture = currentChain.Count > 1,
                IsPromotion = CanPromote(currentRow, piece.Color)
            };
            allCaptures.Add(move);
        }
    }

    /// <summary>
    /// Получает простые ходы (без взятия) для дамки.
    /// В русских шашках дамка ходит на любое количество клеток по диагонали
    /// в любом из 4 направлений, пока не встретит препятствие.
    /// </summary>
    private List<MoveModel> GetKingSimpleMoves(PieceModel?[][] board, int row, int col, PieceModel piece)
    {
        var moves = new List<MoveModel>();
        int[][] directions = { new[] { -1, -1 }, new[] { -1, 1 }, new[] { 1, -1 }, new[] { 1, 1 } };

        foreach (var dir in directions)
        {
            // Дамка ходит на любое количество клеток по диагонали
            for (int dist = 1; dist < BoardSize; dist++)
            {
                int newRow = row + dir[0] * dist;
                int newCol = col + dir[1] * dist;

                if (!IsInBounds(newRow, newCol)) break;
                if (board[newRow][newCol] != null) break; // Клетка занята — дальше нельзя

                moves.Add(new MoveModel
                {
                    FromRow = row,
                    FromCol = col,
                    ToRow = newRow,
                    ToCol = newCol,
                    PieceColor = piece.Color,
                    PieceTypeBefore = piece.Type
                });
            }
        }

        return moves;
    }

    /// <summary>
    /// Получает все возможные взятия для дамки.
    /// Дамка бьёт через шашку противника на любую свободную клетку за ней.
    /// Поддерживает цепочки взятий. Выбирает максимальные цепочки.
    /// </summary>
    private List<MoveModel> GetKingCaptures(PieceModel?[][] board, int row, int col, PieceModel piece)
    {
        var allCaptures = new List<MoveModel>();
        var visited = new HashSet<(int, int)>();
        FindKingCaptureChains(board, row, col, piece, row, col,
            new List<ChainStep>(), new List<CapturedPieceInfo>(), visited, allCaptures);

        // Выбираем максимальные цепочки (правило большинства)
        if (allCaptures.Count > 0)
        {
            int maxCaptures = allCaptures.Max(m => m.CapturedPieces.Count);
            allCaptures = allCaptures.Where(m => m.CapturedPieces.Count == maxCaptures).ToList();
        }

        return allCaptures;
    }

    /// <summary>
    /// Рекурсивно ищет все возможные цепочки взятий для дамки.
    /// Дамка ищет шашку противника по диагонали, затем может приземлиться
    /// на любую свободную клетку за ней. Использует backtracking.
    /// </summary>
    private void FindKingCaptureChains(PieceModel?[][] board, int startRow, int startCol,
        PieceModel piece, int currentRow, int currentCol,
        List<ChainStep> currentChain, List<CapturedPieceInfo> capturedSoFar,
        HashSet<(int, int)> visited, List<MoveModel> allCaptures)
    {
        int[][] directions = { new[] { -1, -1 }, new[] { -1, 1 }, new[] { 1, -1 }, new[] { 1, 1 } };
        bool foundCapture = false;

        foreach (var dir in directions)
        {
            // Ищем шашку противника по диагонали
            for (int dist = 1; dist < BoardSize; dist++)
            {
                int midRow = currentRow + dir[0] * dist;
                int midCol = currentCol + dir[1] * dist;

                if (!IsInBounds(midRow, midCol)) break;

                var midPiece = board[midRow][midCol];

                // Своя шашка — дальше нельзя
                if (midPiece != null && midPiece.Color == piece.Color) break;
                // Уже захваченная шашка — дальше нельзя
                if (midPiece != null && visited.Contains((midRow, midCol))) break;

                if (midPiece != null && midPiece.Color != piece.Color)
                {
                    // Нашли шашку противника, ищем свободные клетки за ней для приземления
                    for (int landDist = 1; landDist < BoardSize; landDist++)
                    {
                        int landRow = midRow + dir[0] * landDist;
                        int landCol = midCol + dir[1] * landDist;

                        if (!IsInBounds(landRow, landCol)) break;
                        // Клетка занята (кроме стартовой позиции) — дальше нельзя
                        if (board[landRow][landCol] != null &&
                            !(landRow == startRow && landCol == startCol))
                            break;

                        foundCapture = true;

                        var step = new ChainStep
                        {
                            FromRow = currentRow,
                            FromCol = currentCol,
                            ToRow = landRow,
                            ToCol = landCol,
                            CapturedRow = midRow,
                            CapturedCol = midCol,
                            CapturedPiece = midPiece.Clone()
                        };

                        var capturedInfo = new CapturedPieceInfo
                        {
                            Row = midRow,
                            Col = midCol,
                            Piece = midPiece.Clone()
                        };

                        var newChain = new List<ChainStep>(currentChain) { step };
                        var newCaptured = new List<CapturedPieceInfo>(capturedSoFar) { capturedInfo };
                        var newVisited = new HashSet<(int, int)>(visited) { (midRow, midCol) };

                        // Временно модифицируем доску для рекурсии
                        var savedMid = board[midRow][midCol];
                        var savedLand = board[landRow][landCol];
                        var savedCurrent = board[currentRow][currentCol];
                        board[midRow][midCol] = null;
                        board[landRow][landCol] = piece;
                        board[currentRow][currentCol] = null;

                        // Рекурсивно ищем продолжение цепочки
                        FindKingCaptureChains(board, startRow, startCol, piece, landRow, landCol,
                            newChain, newCaptured, newVisited, allCaptures);

                        // Восстанавливаем доску (backtracking)
                        board[midRow][midCol] = savedMid;
                        board[landRow][landCol] = savedLand;
                        board[currentRow][currentCol] = savedCurrent;
                    }
                    break; // Не продолжаем за шашкой противника в этом направлении
                }
            }
        }

        // Если продолжения нет и цепочка не пуста — сохраняем результат
        if (!foundCapture && currentChain.Count > 0)
        {
            allCaptures.Add(new MoveModel
            {
                FromRow = startRow,
                FromCol = startCol,
                ToRow = currentRow,
                ToCol = currentCol,
                PieceColor = piece.Color,
                PieceTypeBefore = piece.Type,
                CapturedPieces = new List<CapturedPieceInfo>(capturedSoFar),
                ChainSteps = new List<ChainStep>(currentChain),
                IsChainCapture = currentChain.Count > 1
            });
        }
    }

    /// <summary>
    /// Проверяет, есть ли обязательное взятие у указанного цвета.
    /// </summary>
    public bool MustCapture(PieceModel?[][] board, PieceColor color)
    {
        return HasAnyCapturesForColor(board, color);
    }

    /// <summary>
    /// Проверяет, может ли шашка стать дамкой на указанной строке.
    /// Белые становятся дамками на строке 0, чёрные — на строке 7.
    /// </summary>
    public bool CanPromote(int row, PieceColor color)
    {
        return (color == PieceColor.White && row == 0) ||
               (color == PieceColor.Black && row == BoardSize - 1);
    }

    /// <summary>
    /// Проверяет, закончена ли игра.
    /// Игра заканчивается, если у текущего игрока нет допустимых ходов или шашек.
    /// </summary>
    public bool IsGameOver(PieceModel?[][] board, PieceColor currentTurn)
    {
        var moves = GetValidMoves(board, currentTurn);
        if (moves.Count == 0) return true;

        // Проверяем, есть ли шашки у текущего игрока
        bool hasPieces = false;
        for (int r = 0; r < BoardSize; r++)
            for (int c = 0; c < BoardSize; c++)
                if (board[r][c]?.Color == currentTurn)
                    hasPieces = true;

        return !hasPieces;
    }

    /// <summary>
    /// Определяет результат игры.
    /// Текущий игрок, не имеющий ходов, проигрывает.
    /// </summary>
    public GameStatus GetGameResult(PieceModel?[][] board, PieceColor currentTurn)
    {
        if (!IsGameOver(board, currentTurn))
            return GameStatus.InProgress;

        // Текущий игрок не может ходить — он проиграл
        return currentTurn == PieceColor.White ? GameStatus.BlackWins : GameStatus.WhiteWins;
    }

    /// <summary>
    /// Проверяет, находятся ли координаты в пределах доски.
    /// </summary>
    private bool IsInBounds(int row, int col)
    {
        return row >= 0 && row < BoardSize && col >= 0 && col < BoardSize;
    }
}
