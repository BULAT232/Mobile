using System.Globalization;
using CheckersGame.Models;

namespace CheckersGame.Converters;

/// <summary>
/// Конвертер шашки в текстовый символ Unicode.
/// ⛀ — белая шашка, ⛁ — белая дамка, ⛂ — чёрная шашка, ⛃ — чёрная дамка.
/// </summary>
public class PieceToTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not PieceModel piece)
            return string.Empty;

        return (piece.Color, piece.Type) switch
        {
            (PieceColor.White, PieceType.Man) => "⛀",
            (PieceColor.White, PieceType.King) => "⛁",
            (PieceColor.Black, PieceType.Man) => "⛂",
            (PieceColor.Black, PieceType.King) => "⛃",
            _ => string.Empty
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
