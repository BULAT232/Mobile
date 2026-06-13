using CheckersGame.Models;

namespace CheckersGame.Services;

/// <summary>
/// Поддавки (8×8).
/// Те же правила движения, что и русские шашки,
/// но цель — ПОТЕРЯТЬ все свои шашки. Побеждает тот, у кого не осталось шашек.
/// </summary>
public class GiveawayCheckersRules : ICheckersRules
{
    private readonly RussianCheckersRules _baseRules = new();

    public int BoardSize => 8;
    public GameType GameType => GameType.Giveaway;
    public bool HasFlyingKings => false;

    public PieceModel?[][] InitializeBoard() => _baseRules.InitializeBoard();

    public List<MoveModel> GetValidMoves(PieceModel?[][] board, PieceColor color)
        => _baseRules.GetValidMoves(board, color);

    public List<MoveModel> GetValidMovesForPiece(PieceModel?[][] board, int row, int col)
        => _baseRules.GetValidMovesForPiece(board, row, col);

    public bool MustCapture(PieceModel?[][] board, PieceColor color)
        => _baseRules.MustCapture(board, color);

    public bool CanPromote(int row, PieceColor color)
        => _baseRules.CanPromote(row, color);

    public bool IsGameOver(PieceModel?[][] board, PieceColor currentTurn)
    {
        // Игра окончена, если у текущего игрока нет ходов или нет шашек
        var moves = GetValidMoves(board, currentTurn);
        if (moves.Count == 0) return true;

        // Также проверяем, есть ли шашки у кого-то
        bool hasWhite = false, hasBlack = false;
        for (int r = 0; r < BoardSize; r++)
            for (int c = 0; c < BoardSize; c++)
            {
                if (board[r][c]?.Color == PieceColor.White) hasWhite = true;
                if (board[r][c]?.Color == PieceColor.Black) hasBlack = true;
            }

        return !hasWhite || !hasBlack;
    }

    public GameStatus GetGameResult(PieceModel?[][] board, PieceColor currentTurn)
    {
        if (!IsGameOver(board, currentTurn))
            return GameStatus.InProgress;

        // В поддавках: тот, у кого нет шашек или ходов — ПОБЕЖДАЕТ
        bool hasWhite = false, hasBlack = false;
        for (int r = 0; r < BoardSize; r++)
            for (int c = 0; c < BoardSize; c++)
            {
                if (board[r][c]?.Color == PieceColor.White) hasWhite = true;
                if (board[r][c]?.Color == PieceColor.Black) hasBlack = true;
            }

        // Если у белых нет шашек — белые победили
        if (!hasWhite) return GameStatus.WhiteWins;
        // Если у чёрных нет шашек — чёрные победили
        if (!hasBlack) return GameStatus.BlackWins;

        // Если текущий игрок не может ходить — он побеждает (нет ходов = победа в поддавках)
        var moves = GetValidMoves(board, currentTurn);
        if (moves.Count == 0)
        {
            return currentTurn == PieceColor.White ? GameStatus.WhiteWins : GameStatus.BlackWins;
        }

        return GameStatus.InProgress;
    }
}
