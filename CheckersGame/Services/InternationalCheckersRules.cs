using CheckersGame.Models;

namespace CheckersGame.Services;

/// <summary>
/// Международные шашки (10×10).
/// Летающие дамки, шашки бьют назад.
/// </summary>
public class InternationalCheckersRules : ICheckersRules
{
    public int BoardSize => 10;
    public GameType GameType => GameType.International;
    public bool HasFlyingKings => true;

    public PieceModel?[][] InitializeBoard()
    {
        var board = new PieceModel?[BoardSize][];
        for (int r = 0; r < BoardSize; r++)
        {
            board[r] = new PieceModel?[BoardSize];
            for (int c = 0; c < BoardSize; c++)
            {
                if ((r + c) % 2 == 1)
                {
                    if (r < 4)
                        board[r][c] = new PieceModel(PieceColor.Black);
                    else if (r > 5)
                        board[r][c] = new PieceModel(PieceColor.White);
                }
            }
        }
        return board;
    }

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

        return captures.Count > 0 ? captures : simpleMoves;
    }

    public List<MoveModel> GetValidMovesForPiece(PieceModel?[][] board, int row, int col)
    {
        var piece = board[row][col];
        if (piece == null) return new List<MoveModel>();

        var captures = piece.Type == PieceType.King
            ? GetKingCaptures(board, row, col, piece)
            : GetManCaptures(board, row, col, piece);

        if (HasAnyCapturesForColor(board, piece.Color))
            return captures;

        var simpleMoves = piece.Type == PieceType.King
            ? GetKingSimpleMoves(board, row, col, piece)
            : GetManSimpleMoves(board, row, col, piece);

        return simpleMoves;
    }

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

    private List<MoveModel> GetManSimpleMoves(PieceModel?[][] board, int row, int col, PieceModel piece)
    {
        var moves = new List<MoveModel>();
        int direction = piece.Color == PieceColor.White ? -1 : 1;

        int[][] offsets = { new[] { direction, -1 }, new[] { direction, 1 } };

        foreach (var offset in offsets)
        {
            int newRow = row + offset[0];
            int newCol = col + offset[1];

            if (IsInBounds(newRow, newCol) && board[newRow][newCol] == null)
            {
                moves.Add(new MoveModel
                {
                    FromRow = row,
                    FromCol = col,
                    ToRow = newRow,
                    ToCol = newCol,
                    PieceColor = piece.Color,
                    PieceTypeBefore = piece.Type,
                    IsPromotion = CanPromote(newRow, piece.Color)
                });
            }
        }

        return moves;
    }

    private List<MoveModel> GetManCaptures(PieceModel?[][] board, int row, int col, PieceModel piece)
    {
        var allCaptures = new List<MoveModel>();
        var visited = new HashSet<(int, int)>();
        FindManCaptureChains(board, row, col, piece, row, col,
            new List<ChainStep>(), new List<CapturedPieceInfo>(), visited, allCaptures);

        if (allCaptures.Count > 0)
        {
            int maxCaptures = allCaptures.Max(m => m.CapturedPieces.Count);
            allCaptures = allCaptures.Where(m => m.CapturedPieces.Count == maxCaptures).ToList();
        }

        return allCaptures;
    }

    private void FindManCaptureChains(PieceModel?[][] board, int startRow, int startCol,
        PieceModel piece, int currentRow, int currentCol,
        List<ChainStep> currentChain, List<CapturedPieceInfo> capturedSoFar,
        HashSet<(int, int)> visited, List<MoveModel> allCaptures)
    {
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

            foundCapture = true;

            var step = new ChainStep
            {
                FromRow = currentRow, FromCol = currentCol,
                ToRow = landRow, ToCol = landCol,
                CapturedRow = midRow, CapturedCol = midCol,
                CapturedPiece = midPiece.Clone()
            };

            var capturedInfo = new CapturedPieceInfo { Row = midRow, Col = midCol, Piece = midPiece.Clone() };
            var newChain = new List<ChainStep>(currentChain) { step };
            var newCaptured = new List<CapturedPieceInfo>(capturedSoFar) { capturedInfo };
            var newVisited = new HashSet<(int, int)>(visited) { (midRow, midCol) };

            var savedMid = board[midRow][midCol];
            var savedLand = board[landRow][landCol];
            board[midRow][midCol] = null;
            board[landRow][landCol] = piece;
            board[currentRow][currentCol] = null;

            FindManCaptureChains(board, startRow, startCol, piece, landRow, landCol,
                newChain, newCaptured, newVisited, allCaptures);

            board[midRow][midCol] = savedMid;
            board[landRow][landCol] = savedLand;
            board[currentRow][currentCol] = (currentRow == startRow && currentCol == startCol) ? piece : null;
            if (currentRow != startRow || currentCol != startCol)
                board[currentRow][currentCol] = piece;
        }

        if (!foundCapture && currentChain.Count > 0)
        {
            allCaptures.Add(new MoveModel
            {
                FromRow = startRow, FromCol = startCol,
                ToRow = currentRow, ToCol = currentCol,
                PieceColor = piece.Color,
                PieceTypeBefore = piece.Type,
                CapturedPieces = new List<CapturedPieceInfo>(capturedSoFar),
                ChainSteps = new List<ChainStep>(currentChain),
                IsChainCapture = currentChain.Count > 1,
                IsPromotion = CanPromote(currentRow, piece.Color)
            });
        }
    }

    private List<MoveModel> GetKingSimpleMoves(PieceModel?[][] board, int row, int col, PieceModel piece)
    {
        var moves = new List<MoveModel>();
        int[][] directions = { new[] { -1, -1 }, new[] { -1, 1 }, new[] { 1, -1 }, new[] { 1, 1 } };

        foreach (var dir in directions)
        {
            for (int dist = 1; dist < BoardSize; dist++)
            {
                int newRow = row + dir[0] * dist;
                int newCol = col + dir[1] * dist;

                if (!IsInBounds(newRow, newCol)) break;
                if (board[newRow][newCol] != null) break;

                moves.Add(new MoveModel
                {
                    FromRow = row, FromCol = col,
                    ToRow = newRow, ToCol = newCol,
                    PieceColor = piece.Color,
                    PieceTypeBefore = piece.Type
                });
            }
        }

        return moves;
    }

    private List<MoveModel> GetKingCaptures(PieceModel?[][] board, int row, int col, PieceModel piece)
    {
        var allCaptures = new List<MoveModel>();
        var visited = new HashSet<(int, int)>();
        FindKingCaptureChains(board, row, col, piece, row, col,
            new List<ChainStep>(), new List<CapturedPieceInfo>(), visited, allCaptures);

        if (allCaptures.Count > 0)
        {
            int maxCaptures = allCaptures.Max(m => m.CapturedPieces.Count);
            allCaptures = allCaptures.Where(m => m.CapturedPieces.Count == maxCaptures).ToList();
        }

        return allCaptures;
    }

    private void FindKingCaptureChains(PieceModel?[][] board, int startRow, int startCol,
        PieceModel piece, int currentRow, int currentCol,
        List<ChainStep> currentChain, List<CapturedPieceInfo> capturedSoFar,
        HashSet<(int, int)> visited, List<MoveModel> allCaptures)
    {
        int[][] directions = { new[] { -1, -1 }, new[] { -1, 1 }, new[] { 1, -1 }, new[] { 1, 1 } };
        bool foundCapture = false;

        foreach (var dir in directions)
        {
            for (int dist = 1; dist < BoardSize; dist++)
            {
                int midRow = currentRow + dir[0] * dist;
                int midCol = currentCol + dir[1] * dist;

                if (!IsInBounds(midRow, midCol)) break;

                var midPiece = board[midRow][midCol];
                if (midPiece != null && midPiece.Color == piece.Color) break;
                if (midPiece != null && visited.Contains((midRow, midCol))) break;

                if (midPiece != null && midPiece.Color != piece.Color)
                {
                    for (int landDist = 1; landDist < BoardSize; landDist++)
                    {
                        int landRow = midRow + dir[0] * landDist;
                        int landCol = midCol + dir[1] * landDist;

                        if (!IsInBounds(landRow, landCol)) break;
                        if (board[landRow][landCol] != null &&
                            !(landRow == startRow && landCol == startCol))
                            break;

                        foundCapture = true;

                        var step = new ChainStep
                        {
                            FromRow = currentRow, FromCol = currentCol,
                            ToRow = landRow, ToCol = landCol,
                            CapturedRow = midRow, CapturedCol = midCol,
                            CapturedPiece = midPiece.Clone()
                        };

                        var capturedInfo = new CapturedPieceInfo { Row = midRow, Col = midCol, Piece = midPiece.Clone() };
                        var newChain = new List<ChainStep>(currentChain) { step };
                        var newCaptured = new List<CapturedPieceInfo>(capturedSoFar) { capturedInfo };
                        var newVisited = new HashSet<(int, int)>(visited) { (midRow, midCol) };

                        var savedMid = board[midRow][midCol];
                        var savedLand = board[landRow][landCol];
                        var savedCurrent = board[currentRow][currentCol];
                        board[midRow][midCol] = null;
                        board[landRow][landCol] = piece;
                        board[currentRow][currentCol] = null;

                        FindKingCaptureChains(board, startRow, startCol, piece, landRow, landCol,
                            newChain, newCaptured, newVisited, allCaptures);

                        board[midRow][midCol] = savedMid;
                        board[landRow][landCol] = savedLand;
                        board[currentRow][currentCol] = savedCurrent;
                    }
                    break;
                }
            }
        }

        if (!foundCapture && currentChain.Count > 0)
        {
            allCaptures.Add(new MoveModel
            {
                FromRow = startRow, FromCol = startCol,
                ToRow = currentRow, ToCol = currentCol,
                PieceColor = piece.Color,
                PieceTypeBefore = piece.Type,
                CapturedPieces = new List<CapturedPieceInfo>(capturedSoFar),
                ChainSteps = new List<ChainStep>(currentChain),
                IsChainCapture = currentChain.Count > 1
            });
        }
    }

    public bool MustCapture(PieceModel?[][] board, PieceColor color)
    {
        return HasAnyCapturesForColor(board, color);
    }

    public bool CanPromote(int row, PieceColor color)
    {
        return (color == PieceColor.White && row == 0) ||
               (color == PieceColor.Black && row == BoardSize - 1);
    }

    public bool IsGameOver(PieceModel?[][] board, PieceColor currentTurn)
    {
        var moves = GetValidMoves(board, currentTurn);
        return moves.Count == 0;
    }

    public GameStatus GetGameResult(PieceModel?[][] board, PieceColor currentTurn)
    {
        if (!IsGameOver(board, currentTurn))
            return GameStatus.InProgress;

        return currentTurn == PieceColor.White ? GameStatus.BlackWins : GameStatus.WhiteWins;
    }

    private bool IsInBounds(int row, int col)
    {
        return row >= 0 && row < BoardSize && col >= 0 && col < BoardSize;
    }
}
