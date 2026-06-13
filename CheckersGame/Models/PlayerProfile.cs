using Newtonsoft.Json;

namespace CheckersGame.Models;

/// <summary>
/// Профиль игрока.
/// Хранит имя, рейтинг (ELO), статистику побед/поражений и серии побед.
/// Сериализуется в JSON для сохранения на диск.
/// </summary>
public class PlayerProfile
{
    /// <summary>
    /// Уникальный идентификатор профиля (генерируется автоматически).
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Отображаемое имя игрока.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Цвет аватара игрока в формате HEX (например, "#512BD4").
    /// </summary>
    [JsonProperty("avatarColor")]
    public string AvatarColor { get; set; } = "#512BD4";

    /// <summary>
    /// Рейтинг игрока по системе ELO. Начальное значение — 1000.
    /// Обновляется после каждой игры на основе результата и рейтинга соперника.
    /// </summary>
    [JsonProperty("rating")]
    public int Rating { get; set; } = 1000;

    /// <summary>
    /// Общее количество сыгранных партий.
    /// </summary>
    [JsonProperty("gamesPlayed")]
    public int GamesPlayed { get; set; }

    /// <summary>
    /// Количество побед.
    /// </summary>
    [JsonProperty("wins")]
    public int Wins { get; set; }

    /// <summary>
    /// Количество поражений.
    /// </summary>
    [JsonProperty("losses")]
    public int Losses { get; set; }

    /// <summary>
    /// Количество ничьих.
    /// </summary>
    [JsonProperty("draws")]
    public int Draws { get; set; }

    /// <summary>
    /// Общее количество захваченных шашек за все партии.
    /// </summary>
    [JsonProperty("totalCaptured")]
    public int TotalCaptured { get; set; }

    /// <summary>
    /// Текущая серия побед подряд. Сбрасывается при поражении или ничьей.
    /// </summary>
    [JsonProperty("currentStreak")]
    public int CurrentStreak { get; set; }

    /// <summary>
    /// Лучшая (максимальная) серия побед подряд за всё время.
    /// </summary>
    [JsonProperty("bestStreak")]
    public int BestStreak { get; set; }

    /// <summary>
    /// Дата создания профиля.
    /// </summary>
    [JsonProperty("createdDate")]
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Процент побед (0–100). Вычисляемое свойство, не сериализуется.
    /// Возвращает 0, если не было сыграно ни одной партии.
    /// </summary>
    [JsonIgnore]
    public double WinRate => GamesPlayed > 0 ? (double)Wins / GamesPlayed * 100 : 0;

    /// <summary>
    /// Отображаемое имя с рейтингом в формате "Имя (1000)".
    /// Вычисляемое свойство, не сериализуется.
    /// </summary>
    [JsonIgnore]
    public string DisplayName => $"{Name} ({Rating})";
}
