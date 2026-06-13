using CommunityToolkit.Mvvm.ComponentModel;

namespace CheckersGame.Models;

/// <summary>
/// Модель клетки на шашечной доске.
/// Содержит координаты клетки, информацию о находящейся на ней шашке
/// и флаги визуального состояния (выделение, подсветка допустимых ходов и т.д.).
/// Наследует ObservableObject для автоматического уведомления UI об изменениях свойств.
/// </summary>
public partial class CellModel : ObservableObject
{
    /// <summary>
    /// Номер строки клетки на доске (0 — верхний ряд).
    /// </summary>
    [ObservableProperty]
    private int _row;

    /// <summary>
    /// Номер столбца клетки на доске (0 — левый столбец).
    /// </summary>
    [ObservableProperty]
    private int _col;

    /// <summary>
    /// Шашка, стоящая на данной клетке. Null, если клетка пуста.
    /// </summary>
    [ObservableProperty]
    private PieceModel? _piece;

    /// <summary>
    /// Является ли клетка игровой (тёмной).
    /// На стандартной доске игровые клетки — те, у которых (row + col) % 2 == 1.
    /// </summary>
    [ObservableProperty]
    private bool _isPlayable;

    /// <summary>
    /// Флаг подсветки клетки (общая подсветка, например, для анимации).
    /// </summary>
    [ObservableProperty]
    private bool _isHighlighted;

    /// <summary>
    /// Флаг выделения клетки (игрок выбрал шашку на этой клетке).
    /// </summary>
    [ObservableProperty]
    private bool _isSelected;

    /// <summary>
    /// Флаг допустимой цели хода (клетка подсвечивается как возможный ход для выбранной шашки).
    /// </summary>
    [ObservableProperty]
    private bool _isValidTarget;

    /// <summary>
    /// Флаг последнего хода (клетка участвовала в последнем выполненном ходе).
    /// </summary>
    [ObservableProperty]
    private bool _isLastMove;

    /// <summary>
    /// Конструктор по умолчанию.
    /// </summary>
    public CellModel()
    {
    }

    /// <summary>
    /// Создаёт клетку с указанными координатами и признаком игровой клетки.
    /// </summary>
    /// <param name="row">Номер строки (0 — верхний ряд).</param>
    /// <param name="col">Номер столбца (0 — левый столбец).</param>
    /// <param name="isPlayable">True, если клетка игровая (тёмная).</param>
    public CellModel(int row, int col, bool isPlayable)
    {
        Row = row;
        Col = col;
        IsPlayable = isPlayable;
    }
}
