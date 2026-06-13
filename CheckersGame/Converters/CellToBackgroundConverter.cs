using System.Globalization;
using CheckersGame.Models;

namespace CheckersGame.Converters;

/// <summary>
/// Конвертер клетки в цвет фона на основе состояния и темы.
/// </summary>
public class CellToBackgroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not CellModel cell)
            return Colors.Transparent;

        // Приоритет подсветки
        if (cell.IsSelected)
            return Color.FromArgb("#FFEB3B"); // Жёлтый — выбранная клетка

        if (cell.IsValidTarget)
            return Color.FromArgb("#81C784"); // Зелёный — допустимый ход

        if (cell.IsLastMove)
            return Color.FromArgb("#64B5F6"); // Голубой — последний ход

        // Базовый цвет клетки
        if (!cell.IsPlayable)
            return Color.FromArgb("#EBECD0"); // Светлая клетка

        return Color.FromArgb("#739552"); // Тёмная клетка
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
