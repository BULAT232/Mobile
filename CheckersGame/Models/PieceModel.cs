using Newtonsoft.Json;

namespace CheckersGame.Models;

/// <summary>
/// Модель шашки на доске.
/// Содержит цвет (белая/чёрная) и тип (простая/дамка).
/// Поддерживает клонирование для сохранения состояния при undo/redo.
/// </summary>
public class PieceModel
{
    /// <summary>
    /// Цвет шашки (White — белая, Black — чёрная).
    /// </summary>
    [JsonProperty("color")]
    public PieceColor Color { get; set; }

    /// <summary>
    /// Тип шашки (Man — простая, King — дамка).
    /// Простая шашка превращается в дамку при достижении последнего ряда.
    /// </summary>
    [JsonProperty("type")]
    public PieceType Type { get; set; }

    /// <summary>
    /// Конструктор по умолчанию (для десериализации).
    /// </summary>
    public PieceModel()
    {
    }

    /// <summary>
    /// Создаёт шашку указанного цвета и типа.
    /// </summary>
    /// <param name="color">Цвет шашки.</param>
    /// <param name="type">Тип шашки (по умолчанию — простая).</param>
    public PieceModel(PieceColor color, PieceType type = PieceType.Man)
    {
        Color = color;
        Type = type;
    }

    /// <summary>
    /// Создаёт глубокую копию шашки.
    /// Используется при клонировании доски для undo/redo.
    /// </summary>
    /// <returns>Новый объект PieceModel с теми же свойствами.</returns>
    public PieceModel Clone()
    {
        return new PieceModel(Color, Type);
    }
}
