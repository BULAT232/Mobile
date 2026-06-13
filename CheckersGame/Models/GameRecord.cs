using Newtonsoft.Json;

namespace CheckersGame.Models;

/// <summary>
/// Запись о завершённой игре.
/// Хранит информацию об участниках, результате, длительности и статистике партии.
/// Сериализуется в JSON для сохранения в историю игр.
/// </summary>
public class GameRecord
{
    /// <summary>
    /// Уникальный идентификатор записи (генерируется автоматически).
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Дата и время проведения игры.
    /// </summary>
    [JsonProperty("date")]
    public DateTime Date { get; set; } = DateTime.Now;

    /// <summary>
    /// Идентификатор первого игрока (белые шашки).
    /// </summary>
    [JsonProperty("player1Id")]
    public string Player1Id { get; set; } = string.Empty;

    /// <summary>
    /// Отображаемое имя первого игрока.
    /// </summary>
    [JsonProperty("player1Name")]
    public string Player1Name { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор второго игрока (чёрные шашки).
    /// </summary>
    [JsonProperty("player2Id")]
    public string Player2Id { get; set; } = string.Empty;

    /// <summary>
    /// Отображаемое имя второго игрока.
    /// </summary>
    [JsonProperty("player2Name")]
    public string Player2Name { get; set; } = string.Empty;

    /// <summary>
    /// Тип игры (русские, международные, бразильские, поддавки).
    /// </summary>
    [JsonProperty("gameType")]
    public GameType GameType { get; set; }

    /// <summary>
    /// Идентификатор победителя. Null в случае ничьей.
    /// </summary>
    [JsonProperty("winnerId")]
    public string? WinnerId { get; set; }

    /// <summary>
    /// Итоговый статус игры (победа белых/чёрных или ничья).
    /// </summary>
    [JsonProperty("status")]
    public GameStatus Status { get; set; }

    /// <summary>
    /// Общее количество ходов в партии.
    /// </summary>
    [JsonProperty("moveCount")]
    public int MoveCount { get; set; }

    /// <summary>
    /// Длительность партии от начала до конца.
    /// </summary>
    [JsonProperty("duration")]
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Количество шашек, захваченных первым игроком (белыми).
    /// </summary>
    [JsonProperty("player1Captured")]
    public int Player1Captured { get; set; }

    /// <summary>
    /// Количество шашек, захваченных вторым игроком (чёрными).
    /// </summary>
    [JsonProperty("player2Captured")]
    public int Player2Captured { get; set; }

    /// <summary>
    /// Отображаемый результат игры в текстовом виде (например, "Иван победил" или "Ничья").
    /// Вычисляемое свойство, не сериализуется.
    /// </summary>
    [JsonIgnore]
    public string ResultDisplay
    {
        get
        {
            if (Status == GameStatus.Draw)
                return "Ничья";
            if (WinnerId == Player1Id)
                return $"{Player1Name} победил";
            if (WinnerId == Player2Id)
                return $"{Player2Name} победил";
            return "Неизвестно";
        }
    }

    /// <summary>
    /// Отображаемый тип игры на русском языке.
    /// Вычисляемое свойство, не сериализуется.
    /// </summary>
    [JsonIgnore]
    public string GameTypeDisplay => GameType switch
    {
        GameType.Russian => "Русские шашки",
        GameType.International => "Международные шашки",
        GameType.Brazilian => "Бразильские шашки",
        GameType.Giveaway => "Поддавки",
        _ => "Неизвестно"
    };

    /// <summary>
    /// Отображаемая длительность в формате "мм:сс" или "ч:мм:сс".
    /// Вычисляемое свойство, не сериализуется.
    /// </summary>
    [JsonIgnore]
    public string DurationDisplay => Duration.TotalHours >= 1
        ? Duration.ToString(@"h\:mm\:ss")
        : Duration.ToString(@"mm\:ss");
}
