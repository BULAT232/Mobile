using Newtonsoft.Json;

namespace CheckersGame.Models;

/// <summary>
/// Настройки приложения.
/// Хранит пользовательские предпочтения: тему доски, подсказки, таймер и звук.
/// Сериализуется в JSON для сохранения на диск.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Тема оформления шашечной доски (классическая, деревянная, тёмная).
    /// </summary>
    [JsonProperty("boardTheme")]
    public BoardTheme BoardTheme { get; set; } = BoardTheme.Classic;

    /// <summary>
    /// Показывать ли подсказки допустимых ходов при выборе шашки.
    /// </summary>
    [JsonProperty("showHints")]
    public bool ShowHints { get; set; } = true;

    /// <summary>
    /// Включён ли таймер по умолчанию при создании новой игры.
    /// </summary>
    [JsonProperty("enableTimer")]
    public bool EnableTimer { get; set; } = false;

    /// <summary>
    /// Количество минут на таймере по умолчанию для каждого игрока.
    /// </summary>
    [JsonProperty("defaultTimerMinutes")]
    public int DefaultTimerMinutes { get; set; } = 10;

    /// <summary>
    /// Включены ли звуковые эффекты (ходы, взятия и т.д.).
    /// </summary>
    [JsonProperty("enableSound")]
    public bool EnableSound { get; set; } = true;
}
