namespace CheckersGame.Models;

/// <summary>
/// Тип игры в шашки.
/// Определяет набор правил, размер доски и особенности игровой механики.
/// </summary>
public enum GameType
{
    /// <summary>Русские шашки — доска 8×8, дамка ходит на любое расстояние, шашки бьют назад.</summary>
    Russian,

    /// <summary>Международные шашки — доска 10×10, летающие дамки, шашки бьют назад.</summary>
    International,

    /// <summary>Бразильские шашки — доска 8×8, правила как международные (летающие дамки).</summary>
    Brazilian,

    /// <summary>Поддавки — доска 8×8, правила русских шашек, но цель — потерять все свои шашки.</summary>
    Giveaway
}

/// <summary>
/// Тип шашки на доске.
/// </summary>
public enum PieceType
{
    /// <summary>Обычная шашка (простая) — ходит только вперёд по диагонали.</summary>
    Man,

    /// <summary>Дамка — получается при достижении последнего ряда, ходит в любом направлении.</summary>
    King
}

/// <summary>
/// Цвет шашки (и сторона игрока).
/// </summary>
public enum PieceColor
{
    /// <summary>Белые шашки — ходят первыми, расположены внизу доски.</summary>
    White,

    /// <summary>Чёрные шашки — ходят вторыми, расположены вверху доски.</summary>
    Black
}

/// <summary>
/// Статус текущей игры.
/// </summary>
public enum GameStatus
{
    /// <summary>Игра ещё не начата.</summary>
    NotStarted,

    /// <summary>Игра в процессе.</summary>
    InProgress,

    /// <summary>Победа белых.</summary>
    WhiteWins,

    /// <summary>Победа чёрных.</summary>
    BlackWins,

    /// <summary>Ничья (по соглашению сторон).</summary>
    Draw
}

/// <summary>
/// Тема оформления шашечной доски.
/// Определяет цветовую схему клеток.
/// </summary>
public enum BoardTheme
{
    /// <summary>Классическая зелёная тема.</summary>
    Classic,

    /// <summary>Деревянная коричневая тема.</summary>
    Wooden,

    /// <summary>Тёмная тема.</summary>
    Dark
}
