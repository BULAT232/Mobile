using System.Globalization;

namespace CheckersGame.Converters;

/// <summary>
/// Конвертер bool в цвет. Параметр: "TrueColor|FalseColor".
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue)
            return Colors.Transparent;

        if (parameter is string colorParam)
        {
            var colors = colorParam.Split('|');
            if (colors.Length == 2)
            {
                var trueColor = Color.FromArgb(colors[0]);
                var falseColor = Color.FromArgb(colors[1]);
                return boolValue ? trueColor : falseColor;
            }
        }

        return boolValue ? Colors.Green : Colors.Red;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
